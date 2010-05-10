using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TDCG;
using TDCG.TAHTool;

namespace TAHHair
{
    public partial class Form1 : Form
    {
        string source_file = null;
        Decrypter decrypter = new Decrypter();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (diaOpen1.ShowDialog() == DialogResult.OK)
            {
                source_file = diaOpen1.FileName;
                decrypter.Load(source_file);
                btnCompress.Enabled = false;
                btnLoad.Enabled = false;
                lbStatus.Text = "Processing...";
                DumpEntries();
                lbStatus.Text = "ok. Loaded";
                btnLoad.Enabled = true;
                btnCompress.Enabled = true;
            }
        }

        Regex re_hair_tsofile = new Regex(@"(B|C)00\.tso$");

        private void DumpEntries()
        {
            gvEntries.Rows.Clear();
            foreach (TAHEntry entry in decrypter.Entries)
            {
                if (entry.flag % 2 == 1)
                    continue;

                string file_name = entry.file_name;

                if (re_hair_tsofile.IsMatch(file_name))
                {
                    string[] row = { entry.file_name, entry.offset.ToString(), entry.length.ToString() };
                    gvEntries.Rows.Add(row);
                }
            }
        }

        private void btnCompress_Click(object sender, EventArgs e)
        {
            lbStatus.Text = "Processing...";
            btnCompress.Enabled = false;
            btnLoad.Enabled = false;
            DumpFiles();
        }

        private void DumpFiles()
        {
            bwCompress.RunWorkerAsync(source_file);
        }

        private void bwCompress_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Encrypter encrypter = new Encrypter();
            encrypter.SourcePath = @".";

            Dictionary<string, TAHEntry> entries = new Dictionary<string, TAHEntry>();

            foreach (TAHEntry entry in decrypter.Entries)
            {
                if (entry.flag % 2 == 1)
                    continue;

                string path = entry.file_name;

                if (re_hair_tsofile.IsMatch(path))
                {
                    string dir  = Path.GetDirectoryName(path).Replace("\\", "/");
                    string basename = Path.GetFileNameWithoutExtension(path);
                    string code = basename.Substring(0, 8);
                    string row  = basename.Substring(9, 1);
                    string col  = "19";
                    string new_basename = code + "_" + row + col;

                    entries[code] = entry;

                    string tso_path = encrypter.SourcePath + "/" + dir + "/" + new_basename + ".tso";
                    encrypter.Add(tso_path);
                }
            }
            
            int entries_count = encrypter.Count;
            int current_index = 0;
            encrypter.GetFileEntryStream = delegate(string path)
            {
                Console.WriteLine("compressing {0}", path);

                string basename = Path.GetFileNameWithoutExtension(path);
                string code = basename.Substring(0, 8);
                string row  = basename.Substring(9, 1);
                string col  = basename.Substring(10, 2);

                TAHEntry entry = entries[code];
                string ext = Path.GetExtension(path).ToLower();
                byte[] data_output;
                decrypter.ExtractResource(entry, out data_output);

                Stream ret_stream = null;
                if (ext == ".tso")
                {
                    MemoryStream tso_stream = new MemoryStream(data_output);
                    ret_stream = new MemoryStream();
                    Process(tso_stream, ret_stream, col);
                    ret_stream.Seek(0, SeekOrigin.Begin);
                }
                current_index++;
                int percent = current_index * 100 / entries_count;
                worker.ReportProgress(percent);
                return ret_stream;
            };
            encrypter.Save(@"tso-" + Path.GetFileName(source_file));
        }

        private void bwCompress_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        private void bwCompress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("completed");
            lbStatus.Text = "ok. Compressed";
            pbStatus.Value = 0;
            btnLoad.Enabled = true;
            btnCompress.Enabled = true;
        }

        public static string GetHairKitPath()
        {
            return @"D:\TechArts3D\mod0416\_HAIR_KIT";
        }

        public bool Process(Stream tso_stream, Stream ret_stream, string col)
        {
            TSOFile tso = new TSOFile();
            tso.Load(tso_stream);

            Regex re_kami = new Regex(@"Kami");
            Regex re_housen = new Regex(@"Housen");
            Regex re_ribon = new Regex(@"Ribon");

            Dictionary<string, TSOTex> texmap = new Dictionary<string, TSOTex>();

            foreach (TSOTex tex in tso.textures)
            {
                texmap[tex.Name] = tex;
            }

            foreach (TSOSubScript sub in tso.sub_scripts)
            {
                Console.WriteLine("sub name {0} file {1}", sub.Name, sub.FileName);

                Shader shader = sub.shader;
                string color_tex_name = shader.ColorTexName;
                string shade_tex_name = shader.ShadeTexName;

                if (re_kami.IsMatch(sub.Name))
                {
                    string sub_path = Path.Combine(GetHairKitPath(), string.Format(@"Cgfx_kami\Cgfxkami_{0}", col));
                    sub.Load(sub_path);

                    string tex_path = Path.Combine(GetHairKitPath(), string.Format(@"image_kami\KIT_BASE_{0}.bmp", col));
                    TSOTex colorTex;
                    if (texmap.TryGetValue(color_tex_name, out colorTex))
                    {
                        colorTex.Load(tex_path);
                    }
                }
                else
                if (re_housen.IsMatch(sub.Name))
                {
                    string sub_path = Path.Combine(GetHairKitPath(), string.Format(@"Cgfx_kage\Cgfxkami_{0}", col));
                    sub.Load(sub_path);

                    string tex_path = Path.Combine(GetHairKitPath(), string.Format(@"image_kage\KIT_KAGE_{0}.bmp", col));
                    TSOTex colorTex;
                    if (texmap.TryGetValue(color_tex_name, out colorTex))
                    {
                        colorTex.Load(tex_path);
                    }
                }
                else
                if (re_ribon.IsMatch(sub.Name))
                {
                    string tex_path = Path.Combine(GetHairKitPath(), string.Format(@"image_ribon\KIT_RIBON_{0}.bmp", col));
                    TSOTex colorTex;
                    if (texmap.TryGetValue(color_tex_name, out colorTex))
                    {
                        colorTex.Load(tex_path);
                    }
                }

                sub.GenerateShader();
                Shader new_shader = sub.shader;
                new_shader.ColorTexName = color_tex_name;
                new_shader.ShadeTexName = shade_tex_name;
                sub.SaveShader();

                Console.WriteLine("shader color tex name {0}", color_tex_name);
                Console.WriteLine("shader shade tex name {0}", shade_tex_name);
            }

            foreach (TSOTex tex in tso.textures)
            {
                Console.WriteLine("tex name {0} file {1}", tex.Name, tex.FileName);
            }

            tso.Save(ret_stream);
            return true;
        }
    }
}
