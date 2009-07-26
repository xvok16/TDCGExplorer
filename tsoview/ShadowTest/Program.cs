using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace ShadowTest
{
public static class Program
{
    [STAThread]
    static void Main(string[] args) 
    {
        Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

        using (Viewer viewer = new Viewer())
        using (ViewerForm form = new ViewerForm())
        {
            if (viewer.InitializeApplication(form))
            {
                form.Show();

                while (form.Created)
                {
                    viewer.Render();
                    Application.DoEvents();
                }
            }
        }
    }
}

public class ViewerForm : Form
{
    public ViewerForm()
    {
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.ClientSize = new Size(640, 480);
        this.Text = "ShadowTest";

        this.MaximumSize = this.Size;
        this.MinimumSize = this.Size;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        //this.Render(); // Render on painting
    }

    protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
    {
        if ((int)(byte)e.KeyChar == (int)System.Windows.Forms.Keys.Escape)
            this.Dispose(); // Esc was pressed
    }
}

public class Viewer : IDisposable
{
    internal Device device;
    internal Effect effect;
    internal Mesh teapot;

    struct SParam
    {
        public Vector3 vCamAngle;
        public Vector3 vLightAngle;
        public float fCamLength;
    }

    float m_fCamX;
    float m_fCamY;
    float m_fLitX;
    float m_fLitY;

    Vector3 g_vLightPos;
    Matrix g_matLightTrans;
    Matrix g_matW;
    Matrix g_matL;
    Matrix g_matV;
    Matrix g_matP;
    VertexBuffer g_pVBuffer;

    // マウスポイントしているスクリーン座標
    private Point lastScreenPoint = Point.Empty;

    private void form_OnMouseDown(object sender, MouseEventArgs e)
    {
        lastScreenPoint.X = e.X;
        lastScreenPoint.Y = e.Y;
    }

    private void form_OnMouseMove(object sender, MouseEventArgs e)
    {
        int dx = e.X - lastScreenPoint.X;
        int dy = e.Y - lastScreenPoint.Y;

        float limit_angle = 80.0f;
        float fRotY = (float)dx * 0.2f;
        float fRotX = (float)dy * 0.2f;

        switch (e.Button)
        {
        case MouseButtons.Left:
            m_fCamY += fRotY;
            m_fCamX += fRotX;
            if (m_fCamX > +limit_angle)
                m_fCamX = +limit_angle;
            else
            if (m_fCamX < -limit_angle)
                m_fCamX = -limit_angle;
            break;
        case MouseButtons.Middle:
            break;
        case MouseButtons.Right:
            m_fLitY += fRotY;
            m_fLitX += fRotX;
            if (m_fLitX > +limit_angle)
                m_fLitX = +limit_angle;
            else
            if (m_fLitX < -limit_angle)
                m_fLitX = -limit_angle;
            break;
        }

        lastScreenPoint.X = e.X;
        lastScreenPoint.Y = e.Y;
    }

    public bool InitializeApplication(Control control)
    {
        control.MouseDown += new MouseEventHandler(form_OnMouseDown);
        control.MouseMove += new MouseEventHandler(form_OnMouseMove);

        PresentParameters pp = new PresentParameters();
        try
        {
            pp.Windowed = true;
            pp.SwapEffect = SwapEffect.Discard;
            pp.BackBufferFormat = Format.X8R8G8B8;
            pp.BackBufferCount = 1;
            pp.EnableAutoDepthStencil = true;
            pp.AutoDepthStencilFormat = DepthFormat.D16;

            int adapter_ordinal = Manager.Adapters.Default.Adapter;

            int ret, quality;
            if (Manager.CheckDeviceMultiSampleType(adapter_ordinal, DeviceType.Hardware, pp.BackBufferFormat, pp.Windowed, MultiSampleType.FourSamples, out ret, out quality))
            {
                pp.MultiSample = MultiSampleType.FourSamples;
                pp.MultiSampleQuality = quality - 1;
            }

            CreateFlags flags = CreateFlags.SoftwareVertexProcessing;
            Caps caps = Manager.GetDeviceCaps(adapter_ordinal, DeviceType.Hardware);
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            if (caps.DeviceCaps.SupportsPureDevice)
                flags |= CreateFlags.PureDevice;
            device = new Device(adapter_ordinal, DeviceType.Hardware, control.Handle, flags, pp);

            device.DeviceResizing += new CancelEventHandler(CancelResize);
        }
        catch (DirectXException ex)
        {
            Console.WriteLine("Error: " + ex);
            return false;
        }

        string effect_file = Path.Combine(Application.StartupPath, @"data\vsm.fx");
        using (FileStream effect_stream = File.OpenRead(effect_file))
        {
            string compile_error;
            effect = Effect.FromStream(device, effect_stream, null, ShaderFlags.None, null, out compile_error);
            if (compile_error != null)
            {
                Console.WriteLine(compile_error);
                return false;
            }
        }

        m_fCamX = 30.0f;
        m_fCamY = 0.0f;
        m_fLitX = -60.0f;
        m_fLitY = 135.0f;

        CustomVertex.PositionNormal[] verts = new CustomVertex.PositionNormal[4];
        verts[0] = new CustomVertex.PositionNormal(-30, -5, +30, 0, 1, 0);
        verts[1] = new CustomVertex.PositionNormal(+30, -5, +30, 0, 1, 0);
        verts[2] = new CustomVertex.PositionNormal(-30, -5, -30, 0, 1, 0);
        verts[3] = new CustomVertex.PositionNormal(+30, -5, -30, 0, 1, 0);

        g_pVBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), 4, device, Usage.Dynamic, CustomVertex.PositionNormal.Format, Pool.Default);
        g_pVBuffer.SetData(verts, 0, LockFlags.None);

        teapot = Mesh.Teapot(device);

        return true;
    }

    private void CancelResize(object sender, CancelEventArgs e)
    {
        Console.WriteLine("CancelResize");
        e.Cancel = true;
    }

    public void Render()
    {
        SParam sParam;
        sParam.vCamAngle = new Vector3( Geometry.DegreeToRadian(m_fCamX), Geometry.DegreeToRadian(m_fCamY), 0.0f);
        sParam.vLightAngle = new Vector3( Geometry.DegreeToRadian(m_fLitX), Geometry.DegreeToRadian(m_fLitY), 0.0f);
        sParam.fCamLength = 80.0f;

        g_matW = Matrix.Scaling(5,5,5);
        g_matL = Matrix.Translation(15,0,0);

        Vector3 vEye = new Vector3(0,0,-sParam.fCamLength);
        Vector3 vCenter = new Vector3(0,0,0);
        Vector3 vUp = new Vector3(0,1,0);
        g_matV = Matrix.RotationYawPitchRoll(sParam.vCamAngle.Y, sParam.vCamAngle.X, 0.0f);
        vEye = Vector3.TransformCoordinate(vEye, g_matV);
        g_matV = Matrix.LookAtLH(vEye, vCenter, vUp);

        g_matP = Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), 4.0f/3.0f, 1.0f, 200.0f);

        device.BeginScene();

        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);

        CalcLightTrans(ref sParam);
        //DrawShadowMap(ref sParam);
        //DrawGauss();
        DrawReceiver();

        device.EndScene();

        device.Present();
        Thread.Sleep(30);
    }

    void CalcLightTrans(ref SParam sParam)
    {
        Vector3 vLight = new Vector3(0,0,1);
        Matrix mat;
        mat = Matrix.RotationYawPitchRoll(sParam.vLightAngle.Y, sParam.vLightAngle.X, 0.0f);
        vLight = Vector3.TransformNormal(vLight, mat);

        {
            Vector3 lp;
            Matrix view, proj;

            lp = vLight * -30.0f;
            g_vLightPos = lp;

            view = Matrix.LookAtLH(lp, new Vector3(0,0,0), new Vector3(0,1,0));
            proj = Matrix.OrthoLH(20, 20, 1, 300);
            g_matLightTrans = view * proj;
        }
    }

    void DrawShadowMap(ref SParam sParam)
    {
        Vector4 lp = new Vector4(g_vLightPos.X, g_vLightPos.Y, g_vLightPos.Z, 1.0f);
        Matrix ls = g_matW * g_matLightTrans;
        effect.SetValue("matLS", ls);
        effect.SetValue("vLightPos", lp);

        effect.Technique = "Tec0_NormalDraw";

        //draw teapot
        Matrix wvp = g_matW * g_matV * g_matP;
        effect.SetValue("matWVP", wvp);

        {
            int npass = effect.Begin(0);
            for (int ipass = 0; ipass < npass; ipass++)
            {
                effect.BeginPass(ipass);
                teapot.DrawSubset(0);
                effect.EndPass();
            }
            effect.End();
        }
    }

    void DrawReceiver()
    {
        effect.Technique = "Tec0_NormalDraw";

        Matrix wvp;
        Matrix ls;

        //draw ground
        wvp = g_matV * g_matP;
        effect.SetValue("matWVP", wvp);
        ls = g_matLightTrans;
        effect.SetValue("matLS", ls);

        device.VertexFormat = CustomVertex.PositionNormal.Format;

        {
            int npass = effect.Begin(0);
            for (int ipass = 0; ipass < npass; ipass++)
            {
                effect.BeginPass(ipass);
                device.SetStreamSource(0, g_pVBuffer, 0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
            }
            effect.End();
        }

        //draw teapot
        wvp = g_matW * g_matV * g_matP;
        effect.SetValue("matWVP", wvp);
        ls = g_matW * g_matLightTrans;
        effect.SetValue("matLS", ls);

        {
            int npass = effect.Begin(0);
            for (int ipass = 0; ipass < npass; ipass++)
            {
                effect.BeginPass(ipass);
                teapot.DrawSubset(0);
                effect.EndPass();
            }
            effect.End();
        }
    }

    public void Dispose()
    {
        if (teapot != null)
            teapot.Dispose();
        if (device != null)
            device.Dispose();
    }
}
}
