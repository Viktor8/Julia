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
            pictureBox1.Image = areaView.GetCurrentView();//Image.FromFile((Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + @"\out.png")); //GetBitmapFromAMP();
            
            //.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\out.png",
            //System.Drawing.Imaging.ImageFormat.Png);
        }



        private Bitmap Zoom(Bitmap origin)
        {
            //Bitmap Result = origin.Clone(r, origin.PixelFormat);

            //Graphics g = Graphics.FromImage(Result);
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return null;
        }
        private void pictureBoxMouseWheel(object sender, MouseEventArgs e)
        {
            bool isShift = (Control.ModifierKeys & Keys.Shift) != 0;
            areaView.ZoomKoef = (isShift ? 1.15f : 1.05f);

            PointF p = new PointF((float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);

            pictureBox1.Image = e.Delta > 0 ? areaView.ZoomIn(p) : areaView.ZoomOut(p);
            //pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            this.Update();

        }


    }

    struct Area
    {
        public double ReMin { get; private set; }
        public double ReSize { get; private set; }
        public double ImMin { get; private set; }
        public double ImSize { get; private set; }

        public Area(double reMin, double reSize, double imMin, double imSize)
        {
            ReMin = reMin;
            ReSize = reSize;
            ImMin = imMin;
            ImSize = imSize;
        }
        public bool IsIncludeToSet(Area ar)
        {
            if (this.ReMin < ar.ReMin)
                return false;
            if (this.ImMin < ar.ImMin)
                return false;
            if (ReMin + ReSize > ar.ReMin + ar.ReSize)
                return false;
            if (ImMin + ImSize > ar.ImMin + ar.ImSize)
                return false;
            return true;
        }
        public PointD GetCentr()
        {
            return new PointD(ReMin + ReSize / 2, ImMin + ImSize / 2);
        }
        public static Area GetDefault()
        {
            return new Area(-1.6, 3.2, -1.6, 3.2);
        }


        public static Area operator *(Area a, double k)
        {
            double reMin = a.ReMin + a.ReSize * 0.5 * (1 - k);
            double imMin = a.ImMin + a.ImSize * 0.5 * (1 - k);
            return new Area(reMin, a.ReSize * k, imMin, a.ImSize * k);
        }
    }
    struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
    public delegate void UpdateBM();

    /// <summary>
    /// Provides zoom. Configured considering the current screen
    /// </summary>
    class AreaView
    {
        class ZoomLogic
        {
            public Bitmap GetDefaultView(Bitmap bm)
            {
                if (bm.Height != bm.Width)
                    throw new Exception("Size of original bitmap should be a square");

                if (bm.Width < ScreenSize.Width || bm.Width < ScreenSize.Height)
                    throw new Exception("Map size should be larger than the screen size for a buffering");

                int x = (bm.Width - ScreenSize.Width) / 2;
                int y = (bm.Height - ScreenSize.Height) / 2;

                Bitmap result = bm.Clone(new Rectangle(new Point(x, y), ScreenSize), bm.PixelFormat);

                return result;
            }



            public event UpdateBM NeedToUpdateOriginalBitmap;

            private PointD GlPos;  //CentrOfCurrArreaOnOriginalArea
            private Area OriginArea;
            private Size ScreenSize { get; }
            private int VisibleRes { get; }
            private int BmRes { get; }
            public float BufferingFactor { get; }
            public float SqBuffFactor { get { return BufferingFactor * BufferingFactor; } }

            public ZoomLogic(Area ar)
            {
                OriginArea = ar;
                GlPos = OriginArea.GetCentr();
                ScreenSize = Screen.PrimaryScreen.Bounds.Size;
                VisibleRes = ScreenSize.Height > ScreenSize.Width ? ScreenSize.Height : ScreenSize.Width;
                BufferingFactor = (float)Math.Sqrt(2);
                BmRes = (int)(VisibleRes * SqBuffFactor);
            }
        }

        public Bitmap Original { get; set; }
        private float zoomKoef;
        public float ZoomKoef
        {
            get
            {
                return zoomKoef;
            }
            set
            {
                if (value <= 1 || value > 2)
                    throw new ArgumentOutOfRangeException("1 < ZoomKoef < 2");
                zoomKoef = value;
            }
        }

        private Area OriginArea;
        private Area CurrArea;
        private PointD GlPos;  //CentrOfCurrArreaOnOriginalArea
        private Size ScreenSize;

        public Bitmap ZoomIn(PointF mousePosition)
        {
            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > 1.0 || mousePosition.Y > 1.0)
                throw new ArgumentOutOfRangeException("Mouse position should be 0<x,y<1");

            double CentrRe = CurrArea.ReMin + (CurrArea.ReSize / 4) / ZoomKoef +
                (CurrArea.ReSize / 4 * mousePosition.X) / zoomKoef;
            double CentrIm = CurrArea.ImMin + (CurrArea.ImSize / 4) / ZoomKoef +
                (CurrArea.ImSize / 4 * mousePosition.Y) / zoomKoef;

            double reSize = CurrArea.ReSize / zoomKoef;
            double imSize = CurrArea.ImSize / zoomKoef;

            Area t = new Area(CentrRe - reSize / 2, reSize, CentrIm - imSize / 2, imSize);
            Bitmap result = null;
            try
            {
                result = GetBmForArea(t);
            }
            catch (ArgumentOutOfRangeException)
            {


            }
            catch (ArgumentException)
            {


            }
            return result;

        }
        public Bitmap ZoomOut(PointF mousePosition)
        {
            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > 1.0 || mousePosition.Y > 1.0)
                throw new ArgumentOutOfRangeException("Mouse position should be 0<x,y<1");

            double CentrRe = CurrArea.ReMin + (CurrArea.ReSize / 4) * ZoomKoef +
                (CurrArea.ReSize / 4 * mousePosition.X) * zoomKoef;
            double CentrIm = CurrArea.ImMin + (CurrArea.ImSize / 4) * ZoomKoef +
                (CurrArea.ImSize / 4 * mousePosition.Y) * zoomKoef;

            double reSize = CurrArea.ReSize * zoomKoef;
            double imSize = CurrArea.ImSize * zoomKoef;

            Area t = new Area(CentrRe - reSize / 2, reSize, CentrIm - imSize / 2, imSize);
            Bitmap result = null;
            try
            {
                result = GetBmForArea(t);
            }
            catch (ArgumentOutOfRangeException)
            {


            }
            catch (ArgumentException)
            {


            }
            return result;
        }

        private Bitmap GetBmForArea(Area ar)
        {
            if (ar.ReSize / OriginArea.ReSize < 1 / BufferingFactor ||
                ar.ReSize / OriginArea.ReSize > BufferingFactor ||
                ar.ImSize / OriginArea.ImSize < 1 / BufferingFactor ||
                ar.ImSize / OriginArea.ImSize > BufferingFactor)
                throw new ArgumentOutOfRangeException();
            if (!ar.IsIncludeToSet(OriginArea))
                throw new ArgumentOutOfRangeException();

            int x = (int)(Original.Width * ((ar.ReMin - OriginArea.ReMin) / OriginArea.ReSize));
            int y = (int)(Original.Height * ((ar.ImMin - OriginArea.ImMin) / OriginArea.ImSize));

            int width = (int)(Original.Width * (ar.ReSize / OriginArea.ReSize));
            int height = (int)(Original.Height * (ar.ImSize / OriginArea.ImSize));

            return Original.Clone(new Rectangle(x, y, width, height), Original.PixelFormat);
            

        }

        internal Image GetCurrentView()
        {
            return GetBmForArea(CurrArea);
        }

        public float BufferingFactor { get; }
        public float SqBuffFactor { get { return BufferingFactor * BufferingFactor; } }

        public unsafe AreaView(Area ar, int iter)
        {

            ScreenSize = Screen.PrimaryScreen.Bounds.Size;
            VisibleRes = ScreenSize.Height > ScreenSize.Width ? ScreenSize.Height : ScreenSize.Width;
            BufferingFactor = (float)Math.Sqrt(2);
            dataRange = (int)(VisibleRes * SqBuffFactor);

            CurrArea = OriginArea = ar * BufferingFactor;
            GlPos = OriginArea.GetCentr();

            Bitmap bm = new Bitmap(dataRange, dataRange, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmd = bm.LockBits(new Rectangle(0, 0, dataRange, dataRange),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);

            Julia.Calculate((byte*)bmd.Scan0.ToInt32(), dataRange, ar.ReMin, ar.ReSize, ar.ImMin, ar.ImSize, iter);
            bm.UnlockBits(bmd);
            Original = bm;
        }



        private int VisibleRes { get; }
        private int dataRange { get; }
    }
}
