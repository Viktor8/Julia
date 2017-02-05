using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace Julia
{
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
        public Area ToSqArea()
        {
            if (ReSize == ImSize)
                return this;
            if (ReSize > ImSize)
            {
                double di = (ReSize - ImSize)/2;
                return new Area(ReMin, ReSize, ImMin - di, ReSize);
            }
            return new Area(ImMin - (ImSize - ReSize) / 2, ImSize, ImMin, ImSize);
        }


        public static Area operator /(Area a, double d)
        {
            return a * (1 / d);
        }
        public static Area operator *(Area a, double k)
        {
            double reMin = a.ReMin + a.ReSize * 0.5 * (1 - k);
            double imMin = a.ImMin + a.ImSize * 0.5 * (1 - k);
            return new Area(reMin, a.ReSize * k, imMin, a.ImSize * k);
        }
        public static Area operator +(Area a, PointD p)
        {
            return new Area(a.ReMin + p.X, a.ReSize, a.ImMin + p.Y, a.ImSize);
        }
        public static Area operator +(Area a, PointF p)
        {
            return a + (new PointD(p.X, p.Y));
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

    enum Zooming
    {
        ZoomIn,
        ZoomOut
    }
    public delegate void UpdateBM();

    /// <summary>
    /// Provides zoom. Configured considering the current screen
    /// </summary>
    class AreaView
    {
        public Bitmap Original { get; set; }
        public float ZoomKoef
        {
            get
            {
                return zoomKoef;
            }
            set
            {
                if (value < 1 || value > 2)
                    throw new ArgumentOutOfRangeException("1 < ZoomKoef < 2");
                zoomKoef = value;
            }
        }
        public Size PBsize { private get; set; }
        public float BufferingFactor { get; }
        public float SqBuffFactor { get { return BufferingFactor * BufferingFactor; } }

        private Area OriginArea;
        private Area CurrArea;
        private Size ScreenSize;
        private float zoomKoef;
        private int VisibleRes;
        private int dataRange;
        int iter;
    
       

        public Bitmap Zoom(PointF mousePosition,Zooming direction)
        {
            Logger.Log("zoom " + direction.ToString());

            if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > 1.0 || mousePosition.Y > 1.0)
                throw new ArgumentOutOfRangeException("Mouse position should be 0<x,y<1");
            Area t;
            if (direction == Zooming.ZoomIn)
            {
                double dx = (mousePosition.X - 0.5) * CurrArea.ReSize * (1 - 1 / zoomKoef);
                double dy = (mousePosition.Y - 0.5) * CurrArea.ImSize * (1 - 1 / zoomKoef);
                t = CurrArea / zoomKoef + new PointD(dx, dy);
            }
            else
            {
                double dx = (mousePosition.X - 0.5) * CurrArea.ReSize * (zoomKoef - 1);
                double dy = (mousePosition.Y - 0.5) * CurrArea.ImSize * (zoomKoef - 1);
                t = CurrArea * zoomKoef + new PointD(dx, dy);
            }
            CurrArea = t;
          
            return GetBmForArea(t);

        }
      
        private Bitmap GetBmForArea(Area ar)
        {
            if (ar.ReSize / OriginArea.ReSize < 1 / BufferingFactor ||
                ar.ReSize / OriginArea.ReSize > BufferingFactor ||
                ar.ImSize / OriginArea.ImSize < 1 / BufferingFactor ||
                ar.ImSize / OriginArea.ImSize > BufferingFactor ||
                !ar.IsIncludeToSet(OriginArea))
            {
                Original = GenerateBitmapForArea(ar);
                OriginArea = ar;
                Logger.Log("RECALC");
            }


            int x = (int)(Original.Width * ((ar.ReMin - OriginArea.ReMin) / OriginArea.ReSize));
            int y = (int)(Original.Height * ((ar.ImMin - OriginArea.ImMin) / OriginArea.ImSize));

            int width = (int)(Original.Width * (ar.ReSize / OriginArea.ReSize));
            int height = (int)(Original.Height * (ar.ImSize / OriginArea.ImSize));

            int s = PBsize.Width > PBsize.Height ? PBsize.Width : PBsize.Height;

            return Original.Clone(new Rectangle(x, y, width, height), Original.PixelFormat);


        }
        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        internal Image GetCurrentView()
        {
            return GetBmForArea(CurrArea);
        }
  
        public AreaView(Area ar, int iter)
        { 
            ScreenSize = Screen.PrimaryScreen.Bounds.Size;
            VisibleRes = ScreenSize.Height > ScreenSize.Width ? ScreenSize.Height : ScreenSize.Width;
            BufferingFactor = (float)Math.Sqrt(2);
            dataRange = (int)(VisibleRes * SqBuffFactor);
            this.iter = iter;

            CurrArea = OriginArea = ar * BufferingFactor;
            
            Original = GenerateBitmapForArea(OriginArea);
        }
        private unsafe Bitmap GenerateBitmapForArea(Area ar)
        {
            Bitmap bm;

            if (Original == null)
                Original = bm = new Bitmap(dataRange, dataRange, PixelFormat.Format32bppArgb);
            var bmd = Original.LockBits(new Rectangle(0, 0, dataRange, dataRange),
                ImageLockMode.ReadWrite, Original.PixelFormat);

            ar = ar.ToSqArea();
            Julia.Calculate((int*)bmd.Scan0.ToInt32(), dataRange, ar.ReMin, ar.ReSize, ar.ImMin, ar.ImSize, iter);
            Original.UnlockBits(bmd);
            return Original;
        }


    }
}
