using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TDCG;

namespace TSOWeight
{
    public partial class Form2 : Form
    {
        public WeightViewer viewer = null;
        
        public Form2()
        {
            InitializeComponent();
        }

        bool pressed = false;

        // マウスポイントしているスクリーン座標
        internal Point lastScreenPoint = Point.Empty;

        // This method handles the mouse down event for all the controls on the form.  
        // When a control has captured the mouse
        // the control's name will be output on label1.
        private void Control_MouseDown(System.Object sender,
            System.Windows.Forms.MouseEventArgs e)
        {
            Control control = (Control)sender;
            pressed = true;
            lastScreenPoint.X = e.X;
            lastScreenPoint.Y = e.Y;
            if (control.Capture)
                Console.WriteLine(control.Name + " has captured the mouse");
            else
                Console.WriteLine(control.Name + " has not captured the mouse");
        }

        /// <summary>
        /// 回転操作時に呼び出されるハンドラ
        /// </summary>
        public event EventHandler RotationEvent;

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - lastScreenPoint.X;
            int dy = e.Y - lastScreenPoint.Y;

            Control control = (Control)sender;
            if (pressed)
            {
                if (control == btnRotX)
                    viewer.RotateXOnScreen(dx, dy);
                else
                if (control == btnRotY)
                    viewer.RotateYOnScreen(dx, dy);
                else
                if (control == btnRotZ)
                    viewer.RotateZOnScreen(dx, dy);

                if (RotationEvent != null)
                    RotationEvent(this, EventArgs.Empty);

                lastScreenPoint.X = e.X;
                lastScreenPoint.Y = e.Y;
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            pressed = false;
            if (control.Capture)
                Console.WriteLine(control.Name + " has captured the mouse");
            else
                Console.WriteLine(control.Name + " has not captured the mouse");
        }
    }
}
