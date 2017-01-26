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
        class ZoomLogic
        {
            public float CurentZoom { get; private set; } = 1;
            public float ZoomKoef { get; set; } = 0.05f;
            public float ZoomKoefSpetial { get; set; } = 0.2f;

            public ZoomLogic()
        }



        Bitmap OriginalBM;
        float CurrentZoom = 1;

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
            pictureBox1.Image = GetBitmapFromAMP();//Image.FromFile((Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + @"\out.png")); //GetBitmapFromAMP();
            OriginalBM = new Bitmap(pictureBox1.Image);
            //.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\out.png",
            //System.Drawing.Imaging.ImageFormat.Png);
        }

        private unsafe Bitmap GetBitmapFromAMP()
        {
            int dataRange = 1000;
            Bitmap bm = new Bitmap(dataRange, dataRange, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmd = bm.LockBits(new Rectangle(0, 0, dataRange, dataRange),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);

            Julia.Calculate((byte*)bmd.Scan0.ToInt32(), dataRange, -2, 4, -2, 4, 1000);
            bm.UnlockBits(bmd);
            return bm;
        }
        private Bitmap GetBitmapManaget()
        {
            Julia jul = new Julia(new Ranges(-1.50197, 1.50197, -1.50197, 1.50197),
                new Size(2000,2000), new System.Numerics.Complex(-0.74543, 0.11301));
            jul.Calculate(300);
            return jul.GetBitmap();
        }

        private Bitmap Zoom(Bitmap origin)
        {
            Size t = OriginalBM.Size;
            Rectangle r = new Rectangle((int)(t.Width*(1-CurrentZoom)/2),
                (int)(t.Height * (1 - CurrentZoom) / 2), 
                (int)(t.Width * CurrentZoom),
                (int)(t.Height * CurrentZoom));
            
        
            Bitmap Result = origin.Clone(r,origin.PixelFormat);
           
            //Graphics g = Graphics.FromImage(Result);
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return Result;
        }
        private void pictureBoxMouseWheel(object sender, MouseEventArgs e)
        {
            bool isShift = (Control.ModifierKeys & Keys.Shift) != 0;
            CurrentZoom *= (1 + (isShift?0.2f:0.05f)*(e.Delta>0?-1f:1f));
            pictureBox1.Image = Zoom(OriginalBM);
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            this.Update();

        }


    }
}
 