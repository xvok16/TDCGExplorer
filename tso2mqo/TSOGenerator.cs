using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Tso2MqoGui
{
    public abstract class TSOGenerator
    {
        private string dir;
        private TSOGeneratorConfig config;
        protected MqoFile mqo;
        protected TSOFile tsoref;
        protected Dictionary<string, TSONode> nodes;
        protected List<TSOMesh> meshes;
        private ImportInfo ii;
        private BinaryWriter bw;
        protected Dictionary<string, MaterialInfo> materials;
        private Dictionary<string, TextureInfo> textures;

        public TSOGenerator(TSOGeneratorConfig config)
        {
            this.config = config;
        }

        public TSOFile LoadTSO(string file)
        {
            TSOFile tso = new TSOFile(file);
            tso.ReadAll();
            return tso;
        }

        private bool SetCurrentDirectory(string dir)
        {
            this.dir = dir;
            Environment.CurrentDirectory = dir;
            return true;
        }

        private bool DoLoadMQO(string mqo_file)
        {
            // MQO読み込み
            mqo = new MqoFile();
            mqo.Load(mqo_file);
            mqo.Dump();
            return true;
        }

        private bool DoLoadXml(string importinfo_file)
        {
            // XML読み込み
            ii = ImportInfo.Load(importinfo_file);

            // 使用マテリアル一覧取得
            materials = new Dictionary<string, MaterialInfo>();
            bool validmap = true;

            foreach (MqoMaterial i in mqo.Materials)
            {
                MaterialInfo mi = new MaterialInfo(dir, i, ii.GetMaterial(i.name));
                validmap &= mi.Valid;
                materials.Add(i.name, mi);
            }

            if (!validmap || config.ShowMaterials)
            {
                if (config.cui)
                    throw new Exception("マテリアルの設定が無効です");

                FormMaterial dlg = new FormMaterial();
                dlg.materials = materials;

                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
            }

            // 使用テクスチャ一覧の取得
            textures = new Dictionary<string, TextureInfo>();

            foreach (MaterialInfo i in materials.Values)
            {
                string name = Path.GetFileNameWithoutExtension(i.diffuse);

                if (!textures.ContainsKey(name))
                    textures.Add(name, new TextureInfo(name, i.diffuse));

                name = Path.GetFileNameWithoutExtension(i.shadow);

                if (!textures.ContainsKey(name))
                    textures.Add(name, new TextureInfo(name, i.shadow));
            }

            return true;
        }

        private bool DoWriteHeader()
        {
            bw.Write(0x314F5354);
            return true;
        }

        private bool DoWriteNodeNames()
        {
            bw.Write(tsoref.nodes.Length);

            nodes   = new Dictionary<string,TSONode>();

            foreach(TSONode i in tsoref.nodes)
            {
                WriteString(bw, i.Name);
                nodes.Add(i.ShortName, i);
            }

            return true;
        }

        private bool DoWriteNodeMatrices()
        {
            bw.Write(tsoref.nodes.Length);

            foreach(TSONode i in tsoref.nodes)
                WriteMatrix(bw, i.Matrix);

            return true;
        }

        private bool DoWriteTextures()
        {
            bw.Write(textures.Count);

            foreach(TextureInfo i in textures.Values)
            {
                string  file= i.file;
                string  name= i.name;

                WriteString(bw, name);
                WriteString(bw, "\"" + Path.GetFileName(file) + "\"");

                // テクスチャの読み込み
                TSOTex  tex = LoadTex(file);
                tex.name    = name;
                bw.Write(tex.Width);
                bw.Write(tex.Height);
                bw.Write(tex.Depth);
                bw.Write(tex.data, 0, tex.data.Length);

                ImportTextureInfo   iti = new ImportTextureInfo(tex);
                ii.textures.Add(iti);

                // テクスチャが同じフォルダにない場合、コピーしておく
                if(Path.GetDirectoryName(file).ToUpper() != dir.ToUpper())
                {
                    iti.File    = Path.Combine(dir, Path.GetFileName(file));
                    File.Copy(file, iti.File, true);
                }
            }

            return true;
        }

        private bool DoWriteEffects()
        {
            bw.Write(ii.effects.Count);

            foreach(ImportEffectInfo i in ii.effects)
            {
                string      file= Path.Combine(dir, i.Name);
                string[]    code= File.ReadAllLines(file, Encoding.Default);

                WriteString(bw, i.Name);
                bw.Write(code.Length);

                foreach(string j in code)
                    WriteString(bw, j.Trim('\r', '\n'));
            }

            return true;
        }

        private bool DoWriteMaterials()
        {
            bw.Write(mqo.Materials.Count);

            foreach(MqoMaterial i in mqo.Materials)
            {
                MaterialInfo    mi  = materials[i.name];
                string[]        code= mi.GetCode();

                WriteString(bw, i.name);
                WriteString(bw, "cgfxShader");
                bw.Write(code.Length);

                foreach(string j in code)
                    WriteString(bw, j.Trim('\r', '\n'));

                ImportMaterialInfo  imi = new ImportMaterialInfo();
                imi.Name                = i.name;
                imi.File                = "cgfxShader";
                ii.materials.Add(imi);

                // コードを保存する
                File.WriteAllLines(Path.Combine(dir, i.name), code);
            }

            return true;
        }

        private bool DoWriteMeshes()
        {
            bw.Write(meshes.Count);

            foreach(TSOMesh i in meshes)
            {
                WriteString(bw, i.Name);
                WriteMatrix(bw, i.Matrix);
                bw.Write(1);
                bw.Write(i.numsubs);

                foreach(TSOSubMesh j in i.sub)
                {
                    bw.Write(j.spec);
                    bw.Write(j.numbones);

                    foreach(int k in j.bones)
                        bw.Write(k);

                    bw.Write(j.numvertices);

                    foreach(Vertex k in j.vertices)
                        WriteVertex(bw, k);
                }
            }

            return true;
        }

        private bool DoOutput(string tsoout_file)
        {
            //----- 出力処理 -----------------------------------------------
            ii.materials.Clear();
            ii.textures.Clear();

            using (FileStream fs = File.OpenWrite(tsoout_file))
            {
                fs.SetLength(0);
                bw = new BinaryWriter(fs);

                DoWriteHeader();
                DoWriteNodeNames();
                DoWriteNodeMatrices();
                DoWriteTextures();
                DoWriteEffects();
                DoWriteMaterials();
                DoGenerateMeshes();
                DoWriteMeshes();
            }

            return true;
        }
        protected abstract bool DoGenerateMeshes();

        private bool DoSaveXml(string importinfo_file)
        {
            // 結果を保存しておく
            ImportInfo.Save(importinfo_file, ii);
            return true;
        }

        protected virtual bool DoCleanup()
        {
            dir = null;
            tsoref = null;
            nodes = null;
            meshes = null;
            mqo = null;
            ii = null;
            bw = null;
            materials = null;
            textures = null;

            System.GC.Collect();
            return true;
        }

        public void Generate(string mqo_file, string tsoref_file, string tsoout_file)
        {
            string dir = Path.GetDirectoryName(mqo_file);
            string importinfo_file = Path.ChangeExtension(mqo_file, ".xml");

            try
            {
                if (!SetCurrentDirectory(dir)) return;
                if (!DoLoadMQO(mqo_file)) return;
                if (!DoLoadRefTSO(tsoref_file)) return;
                if (!DoLoadXml(importinfo_file)) return;
                if (!DoOutput(tsoout_file)) return;
                if (!DoSaveXml(importinfo_file)) return;
            }
            finally
            {
                DoCleanup();
            }
        }
        
        protected abstract bool DoLoadRefTSO(string tsoref);

#region ユーティリティ
        public void WriteString(BinaryWriter bw, string s)
        {
            byte[]  b   = Encoding.Default.GetBytes(s);
            bw.Write(b);
            bw.Write((byte)0);
        }

        public void WriteMatrix(BinaryWriter bw, Matrix44 m)
        {
            bw.Write(m.M11); bw.Write(m.M12); bw.Write(m.M13); bw.Write(m.M14);
            bw.Write(m.M21); bw.Write(m.M22); bw.Write(m.M23); bw.Write(m.M24);
            bw.Write(m.M31); bw.Write(m.M32); bw.Write(m.M33); bw.Write(m.M34);
            bw.Write(m.M41); bw.Write(m.M42); bw.Write(m.M43); bw.Write(m.M44);
        }

        public unsafe void WriteVertex(BinaryWriter bw, Vertex v)
        {
            uint        idx0    = v.Idx;
            byte*       idx     = (byte*)(&idx0);
            List<int>   idxs    = new List<int>(4);
            List<float> wgts    = new List<float>(4);

            if(v.Wgt.x > 0) { idxs.Add(idx[0]); wgts.Add(v.Wgt.x); }
            if(v.Wgt.y > 0) { idxs.Add(idx[1]); wgts.Add(v.Wgt.y); }
            if(v.Wgt.z > 0) { idxs.Add(idx[2]); wgts.Add(v.Wgt.z); }
            if(v.Wgt.w > 0) { idxs.Add(idx[3]); wgts.Add(v.Wgt.w); }

            bw.Write(v.Pos.X); bw.Write(v.Pos.Y); bw.Write(v.Pos.Z);
            bw.Write(v.Nrm.X); bw.Write(v.Nrm.Y); bw.Write(v.Nrm.Z);
            bw.Write(v.Tex.X); bw.Write(v.Tex.Y);

            bw.Write(wgts.Count);

            for(int i= 0, n= idxs.Count; i < n; ++i)
            {
                bw.Write(idxs[i]);
                bw.Write(wgts[i]);
            }
        }
#endregion
#region テクスチャ処理
        public TSOTex   LoadTex(string file)
        {
            string  ext = Path.GetExtension(file).ToUpper();
            TSOTex  tex;

            switch(ext)
            {
            case ".TGA":    tex= LoadTarga(file);   break;
            case ".BMP":    tex= LoadBitmap(file);  break;
            default:        throw new Exception("Unsupported texture file: " + file);
            }

            for(int i= 0, n= tex.data.Length; i < n; i+=tex.Depth)
            {
                byte    b       = tex.data[i+0];
                tex.data[i+0]   = tex.data[i+2];
                tex.data[i+2]   = b;
            }

            return tex;
        }

        public unsafe TSOTex   LoadTarga(string file)
        {
            using(FileStream fs= File.OpenRead(file))
            {
                BinaryReader        br      = new BinaryReader(fs);
                TARGA_HEADER        header;

                Marshal.Copy(br.ReadBytes(sizeof(TARGA_HEADER)), 0, (IntPtr)(&header), sizeof(TARGA_HEADER));

                if(header.imagetype != 0x02)    throw new Exception("Invalid imagetype: " + file);
                if(header.depth     != 24
                && header.depth     != 32)      throw new Exception("Invalid depth: " + file);
                
                TSOTex      tex = new TSOTex();
                tex.depth       = header.depth  / 8;
                tex.width       = header.width;
                tex.height      = header.height;
                tex.File        = file;
                tex.data        = br.ReadBytes(tex.width * tex.height * tex.depth);

                return tex;
            }
        }

        public unsafe TSOTex   LoadBitmap(string file)
        {
            using(FileStream fs= File.OpenRead(file))
            {
                BinaryReader        br      = new BinaryReader(fs);
                BITMAPFILEHEADER    bfh;
                BITMAPINFOHEADER    bih;

                Marshal.Copy(br.ReadBytes(sizeof(BITMAPFILEHEADER)), 0, (IntPtr)(&bfh), sizeof(BITMAPFILEHEADER));
                Marshal.Copy(br.ReadBytes(sizeof(BITMAPINFOHEADER)), 0, (IntPtr)(&bih), sizeof(BITMAPINFOHEADER));

                if(bfh.bfType != 0x4D42)        throw new Exception("Invalid imagetype: " + file);
                if(bih.biBitCount != 24
                && bih.biBitCount != 32)        throw new Exception("Invalid depth: " + file);
                
                TSOTex      tex = new TSOTex();
                tex.depth       = bih.biBitCount  / 8;
                tex.width       = bih.biWidth;
                tex.height      = bih.biHeight;
                tex.File        = file;
                tex.data        = br.ReadBytes(tex.width * tex.height * tex.depth);

                return tex;
            }
        }
#endregion
    }

    public class TSOGeneratorOneBone : TSOGenerator
    {
        public Dictionary<string, string> ObjectBoneNames = new Dictionary<string, string>();

        public TSOGeneratorOneBone(TSOGeneratorConfig config)
            : base(config)
        {
        }

        protected override bool DoLoadRefTSO(string tsoref_file)
        {
            // 参照TSOロード
            tsoref = LoadTSO(tsoref_file);
            return true;
        }

        protected override bool DoGenerateMeshes()
        {
            meshes  = new List<TSOMesh>();

            foreach(MqoObject i in mqo.Objects)
            {
                if(i.name.ToLower() == "bone")
                    continue;

                Console.WriteLine("object:" + i.name);

                // 法線生成
                Point3[]        nrm = new Point3[i.vertices.Count];
                
                foreach(MqoFace j in i.faces)
                {
                    Point3  v1  = Point3.Normalize(i.vertices[j.b] - i.vertices[j.a]);
                    Point3  v2  = Point3.Normalize(i.vertices[j.c] - i.vertices[j.b]);
                    Point3  n   = Point3.Normalize(Point3.Cross(v1, v2));
                    nrm[j.a]    -=n;
                    nrm[j.b]    -=n;
                    nrm[j.c]    -=n;
                }

                for(int j= 0; j < nrm.Length; ++j)
                    nrm[j]      = Point3.Normalize(nrm[j]);

                // ボーン情報作成
                uint                idx     = 0x00000000;
                Point4              wgt     = new Point4(1, 0, 0, 0);
                int[]               bones   = new int[1];
                string              bone    = ObjectBoneNames[i.name];
                bones[0]                    = nodes[bone].ID;

                // マテリアル別に処理を実行
                List<ushort>        indices = new List<ushort>();
                VertexHeap<Vertex>  vh      = new VertexHeap<Vertex>();
                List<TSOSubMesh>    subs    = new List<TSOSubMesh>();

                Console.WriteLine("  vertices bone_indices");
                Console.WriteLine("  -------- ------------");

                for(int j= 0, n= materials.Count; j < n; ++j)
                {
                    int mtl = j;
                    indices.Clear();

                    foreach(MqoFace f in i.faces)
                    {
                        if(f.mtl != mtl)
                            continue;

                        Vertex  va  = new Vertex(i.vertices[f.a], wgt, idx, nrm[f.a], new Point2(f.ta.x, 1-f.ta.y));
                        Vertex  vb  = new Vertex(i.vertices[f.b], wgt, idx, nrm[f.b], new Point2(f.tb.x, 1-f.tb.y));
                        Vertex  vc  = new Vertex(i.vertices[f.c], wgt, idx, nrm[f.c], new Point2(f.tc.x, 1-f.tc.y));

                        indices.Add(vh.Add(va));
                        indices.Add(vh.Add(vc));
                        indices.Add(vh.Add(vb));
                    }

                    if(indices.Count == 0)
                        continue;

                    // フェイス最適化
                    ushort[]    nidx    = NvTriStrip.Optimize(indices.ToArray());

                    // サブメッシュ生成
                    Vertex[]    verts= vh.verts.ToArray();
                    TSOSubMesh  sub = new TSOSubMesh();
                    sub.spec        = mtl;
                    sub.numbones    = bones.Length;
                    sub.bones       = bones;
                    sub.numvertices = nidx.Length;
                    sub.vertices    = new Vertex[nidx.Length];
                   
                    for(int k= 0; k < nidx.Length; ++k)
                        sub.vertices[k] = verts[nidx[k]];

                    Console.WriteLine("  {0,8} {1,12}", sub.vertices.Length, sub.bones.Length);

                    subs.Add(sub);
                }

                // メッシュ生成
                TSOMesh mesh    = new TSOMesh();
                mesh.name       = i.name;
                mesh.numsubs    = subs.Count;
                mesh.sub        = subs.ToArray();
                mesh.matrix     = Matrix44.Identity;
                mesh.effect     = 0;
                meshes.Add(mesh);
            }

            return true;
        }

    }
    
    public unsafe class TSOGeneratorRefBone : TSOGenerator
    {
        private List<Vertex> vlst;
        private PointCluster pc;

        public TSOGeneratorRefBone(TSOGeneratorConfig config)
            : base(config)
        {
        }

        private void CreatePointCluster(TSOFile tso)
        {
            vlst = new List<Vertex>();

            foreach (TSOMesh i in tso.meshes)
                foreach (TSOSubMesh j in i.sub)
                    vlst.AddRange(j.vertices);

            pc = new PointCluster(vlst.Count);

            foreach (Vertex i in vlst)
                pc.Add(i.Pos.x, i.Pos.y, i.Pos.z);

            pc.Clustering();
        }

        protected override bool DoLoadRefTSO(string tsoref_file)
        {
            // 参照TSOロード
            tsoref = LoadTSO(tsoref_file);

            foreach (TSOMesh i in tsoref.meshes)
                foreach (TSOSubMesh j in i.sub)
                {
                    int[] bones = j.bones;

                    for (int k = 0, n = j.numvertices; k < n; ++k)
                    {
                        // ボーンをグローバルな番号に変換
                        uint idx0 = j.vertices[k].Idx;
                        byte* idx = (byte*)(&idx0);
                        idx[0] = (byte)bones[idx[0]];
                        idx[1] = (byte)bones[idx[1]];
                        idx[2] = (byte)bones[idx[2]];
                        idx[3] = (byte)bones[idx[3]];
                        j.vertices[k].Idx = idx0;
                    }
                }

            CreatePointCluster(tsoref);
            return true;
        }

        protected override bool DoGenerateMeshes()
        {
            meshes  = new List<TSOMesh>();

            foreach(MqoObject i in mqo.Objects)
            {
                if(i.name.ToLower() == "bone")
                    continue;

                Console.WriteLine("object:" + i.name);

                // 一番近い頂点への参照
                List<int>       vref= new List<int>(i.vertices.Count);

                foreach(Point3 j in i.vertices)
                    vref.Add(pc.NearestIndex(j.x, j.y, j.z));

                // 法線生成
                Point3[]        nrm = new Point3[i.vertices.Count];
                
                foreach(MqoFace j in i.faces)
                {
                    Point3  v1  = Point3.Normalize(i.vertices[j.b] - i.vertices[j.a]);
                    Point3  v2  = Point3.Normalize(i.vertices[j.c] - i.vertices[j.b]);
                    Point3  n   = Point3.Normalize(Point3.Cross(v1, v2));

                    nrm[j.a]    -=n;
                    nrm[j.b]    -=n;
                    nrm[j.c]    -=n;
                }

                for(int j= 0; j < nrm.Length; ++j)
                    nrm[j]      = Point3.Normalize(nrm[j]);

                // フェイスの組成
                List<int>               faces1  = new List<int>();
                List<int>               faces2  = new List<int>();
              //int[]                   bonecnv = new int[tsor.nodes.Length];   // ボーン変換テーブル
                VertexHeap<Vertex>      vh      = new VertexHeap<Vertex>();
                Vertex[]                v       = new Vertex[3];
                List<int>               bones   = new List<int>(16);
                List<ushort>            indices = new List<ushort>();
                Dictionary<int, int>    selected= new Dictionary<int,int>();
                Dictionary<int, int>    work    = new Dictionary<int,int>();
                List<TSOSubMesh>        subs    = new List<TSOSubMesh>();

                for(int j= 0, n= i.faces.Count; j < n; ++j)
                    faces1.Add(j);

#region ボーンパーティション
                Console.WriteLine("  vertices bone_indices");
                Console.WriteLine("  -------- ------------");

                while (faces1.Count > 0)
                {
                    int                 mtl     = i.faces[faces1[0]].mtl;
                    selected.Clear();
                    indices .Clear();
                    vh      .Clear();
                    bones   .Clear();

                    foreach(int j in faces1)
                    {
                        MqoFace         f       = i.faces[j];

                        if(f.mtl != mtl)
                        {
                            faces2.Add(j);
                            continue;
                        }

                        v[0]                    = vlst[vref[f.a]];
                        v[1]                    = vlst[vref[f.b]];
                        v[2]                    = vlst[vref[f.c]];

                        work.Clear();

                        for(int k= 0; k < 3; ++k)
                        {
                            Vertex      vv      = v[k];
                            UInt32      idx0    = vv.Idx;
                            Point4      wgt0    = vv.Wgt;
                            byte*       idx     = (byte*)(&idx0);
                            float*      wgt     = (float*)(&wgt0);

                            for(int l= 0; l < 4; ++l)
                            {
                                if(wgt[l] <= float.Epsilon)         continue;
                                if(selected.ContainsKey(idx[l]))    continue;
                                
                                if(!work.ContainsKey(idx[l]))
                                    work.Add(idx[l], 0);
                            }
                        }

                        if (selected.Count + work.Count > 16)
                        {
                            faces2.Add(j);
                            continue;
                        }

                        // ボーンリストに足してvalid
                        foreach(KeyValuePair<int, int> l in work)
                        {
                            selected.Add(l.Key, selected.Count);    // ボーンテーブルに追加
                            bones.Add(l.Key);
                        }

                        // \todo 点の追加
                        Vertex  va  = new Vertex(i.vertices[f.a], v[0].Wgt, v[0].Idx, nrm[f.a], new Point2(f.ta.x, 1-f.ta.y));
                        Vertex  vb  = new Vertex(i.vertices[f.b], v[1].Wgt, v[1].Idx, nrm[f.b], new Point2(f.tb.x, 1-f.tb.y));
                        Vertex  vc  = new Vertex(i.vertices[f.c], v[2].Wgt, v[2].Idx, nrm[f.c], new Point2(f.tc.x, 1-f.tc.y));

                        indices.Add(vh.Add(va));
                        indices.Add(vh.Add(vc));
                        indices.Add(vh.Add(vb));
                    }

                    // フェイス最適化
                    ushort[]    nidx    = NvTriStrip.Optimize(indices.ToArray());

                    // 頂点のボーン参照ローカルに変換
                    Vertex[]    verts   = vh.verts.ToArray();

                    for(int j= 0; j < verts.Length; ++j)
                    {
                        uint        idx0= verts[j].Idx;
                        byte*       idx = (byte*)(&idx0);
                        Point4      wgt0= verts[j].Wgt;
                        float*      wgt = (float*)(&wgt0);

                        for(int k= 0; k < 4; ++k)
                            if(wgt[k] > float.Epsilon)
                                idx[k]  = (byte)selected[idx[k]];

                        verts[j].Idx    = idx0;
                    }

                    // サブメッシュ生成
                    TSOSubMesh  sub = new TSOSubMesh();
                    sub.spec        = mtl;
                    sub.numbones    = bones.Count;
                    sub.bones       = bones.ToArray();
                    sub.numvertices = nidx.Length;
                    sub.vertices    = new Vertex[nidx.Length];
                   
                    for(int j= 0; j < nidx.Length; ++j)
                        sub.vertices[j] = verts[nidx[j]];

                    Console.WriteLine("  {0,8} {1,12}", sub.vertices.Length, sub.bones.Length);

                    subs.Add(sub);

                    // 次の周回
                    List<int>   t   = faces1;
                    faces1          = faces2;
                    faces2          = t;
                    t.Clear();
                }
#endregion
                // \todo TSOMesh生成
                TSOMesh mesh    = new TSOMesh();
                mesh.name       = i.name;
                mesh.numsubs    = subs.Count;
                mesh.sub        = subs.ToArray();
                mesh.matrix     = Matrix44.Identity;
                mesh.effect     = 0;
                meshes.Add(mesh);
            }

            return true;
        }

        protected override bool DoCleanup()
        {
            pc = null;
            vlst = null;
            return base.DoCleanup();
        }
    }

    public class TextureInfo
    {
        public string name;
        public string file;

        public TextureInfo(string name, string file)
        {
            this.name   = name;
            this.file   = file;
        }
    }

    public class MaterialInfo
    {
        public string           name;
        public string           shader;
        public string           diffuse;
        public string           shadow;
      //public Dictionary<string, string>   parameters;

        public MaterialInfo(string path, MqoMaterial mqom, ImportMaterialInfo impm)
        {
            name    = mqom.name;
            diffuse = mqom.tex;

            if(impm != null)
            {
                string  file= Path.Combine(path, impm.Name);

                if(File.Exists(file))
                    shader          = file;

                if(impm.shadow != null)
                {
                    file        = Path.Combine(path, impm.shadow.File);

                    if(File.Exists(file))
                        shadow  = file;
                }
            }
        }

        public bool   Valid
        {
            get
            {
                return File.Exists(shader)
                    && File.Exists(diffuse)
                    && File.Exists(shadow);
            }
        }

        public string[] GetCode()
        {
            TSOMaterialCode code= TSOMaterialCode.GenerateFromFile(shader);
            List<string>    line= new List<string>();

            code.SetValue("ColorTex", Path.GetFileNameWithoutExtension(diffuse));
            code.SetValue("ShadeTex", Path.GetFileNameWithoutExtension(shadow));

            foreach(KeyValuePair<string, TSOParameter> i in code)
                line.Add(i.Value.ToString());

            return line.ToArray();
        }

        public string Name           { get { return name;    } }
        
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        [DisplayNameAttribute("シェーダー設定ファイル")]
        public string ShaderFile     { get { return shader;  } set { shader  = value; } }
        
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        [DisplayNameAttribute("テクスチャ：カラー")]
        public string DiffuseTexture { get { return diffuse; } set { diffuse = value; } }

        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        [DisplayNameAttribute("テクスチャ：シェーティング")]
        public string ShadowTexture  { get { return shadow;  } set { shadow  = value; } }
    }
}
