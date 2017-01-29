using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Julia
{
    public partial class Form1 : Form
    {

        AreaView areaView;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = new Size(this.Size.Width - 17, this.Size.Height - 38);
        }

        private unsafe void Form1_Load(object sender, EventArgs e)
        {

            areaView = new AreaView(Area.GetDefault(),1000);
            areaView.PBsize = pictureBox1.Size;
            pictureBox1.Image = areaView.GetCurrentView();
        }

        private void pictureBoxMouseWheel(object sender, MouseEventArgs e)
        {
            bool isShift = (Control.ModifierKeys & Keys.Shift) != 0;
            areaView.ZoomKoef = (isShift ? 1.15f : 1.05f);

            PointF p = new PointF((float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);

            pictureBox1.Image = areaView.Zoom(p, e.Delta > 0 ? Zooming.ZoomIn : Zooming.ZoomOut);

      

            //pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            this.Update();

        }
    }   
}
