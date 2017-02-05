using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Julia
{
    class Julia
    {
        [DllImport("MandelDll.dll")]
        public extern static unsafe void Calculate(int* scan0, int dataSize,
    double xMin, double xSize, double yMin, double ySize, int iterations);

        Complex[,] area;
        short[,] iterationCount;
        double R;
        int MaxCalculationIter;
        int MaxResultIter;
        Ranges workDiap;
        Complex C;


        public Julia(Ranges WorkArea, Size MapResolution, Complex c)
        {
            workDiap = WorkArea;
            area = new Complex[MapResolution.Width, MapResolution.Height];

            this.C = c;
            R = FindR(C);
            CheckDiap(R);
            FeelArea(area, workDiap);
        }
        public short[,] Calculate(int Iteration)
        {
            MaxCalculationIter = Iteration;
            iterationCount = GetIterationMap(area, C, R, Iteration);

            return iterationCount;
        }
        public Bitmap GetBitmap()
        {
            Dictionary<int, Color> Colors = GetPalette();
            Bitmap result = new Bitmap(iterationCount.GetLength(0), iterationCount.GetLength(1));
            for (int i = 0; i < result.Height - 1; i++)
                for (int j = 0; j < result.Width; j++)
                {
                    int key = iterationCount[j, i];
                    result.SetPixel(j, i, Colors[key]);
                }
            //result.SetPixel(j, i, GetColor(area[j, i], iterationCount[j, i]));

            Image im = result;
            im.Save("im.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return result;
        }

        private Dictionary<int, Color> GetPalette()
        {
            Dictionary<int, Color> result = new Dictionary<int, Color>();
            Color t = Color.AliceBlue;
            for (int i = 0; i <= MaxResultIter; i++)
            {
                //result.Add(i, t.FromAHSV(255, (255-255*(((float)i) / MaxResultIter)), 1, 1));
                int c = (int)(255 * (((float)i) / MaxResultIter));
                result.Add(i, Color.FromArgb(255, c, c, c));
            }
            return result;
        }

        #region Private
        Color GetColor(Complex z, int it)
        {
            double val = ((double)(it)) / (MaxResultIter);
            byte Red = (byte)(val * 255);
            byte G = (byte)((1 - val) * 255);
            byte B = (byte)(z.Magnitude > R ? 255 : 255 * z.Magnitude / R);

            //return z.Magnitude > R ? Color.Black : Color.White;


            return Color.FromArgb(255, Red, G, B);
        }


        private short[,] GetIterationMap(Complex[,] Area, Complex c, double r, int maxIter)
        {
            short[,] iterationMap = new short[Area.GetLength(0), Area.GetLength(1)];
            Parallel.For(0, Area.GetLength(0), (i) =>
            {
                for (int j = 0; j < Area.GetLength(1); j++)
                {
                    iterationMap[i, j] = (short)core(area[i, j]);
                    if (iterationMap[i, j] > MaxResultIter)
                        MaxResultIter = iterationMap[i, j];
                }
            });
            return iterationMap;
        }

        int core(Complex z)
        {
            double y = z.Imaginary;
            double x = z.Real;
            double t = 0;
            int iteration = 0;
            while ((x <= 2.0) & (y <= 2.0) & (x >= -2.0) & (y >= -2.0) & (iteration < MaxCalculationIter))
            {
                t = x;
                x = x * x - y * y - 0.74543;
                y = 2 * y * t + 0.11301;
                iteration++;
            }
         
            return iteration;
        }
        int SqPolyIteration(Complex z)
        {
            int iterCount = 0;
            Complex z0 = z;
            for (int i = 0; i < MaxCalculationIter; i++)
            {
                if (z.Magnitude > R)
                    break;
                z = z * z + C;
                iterCount++;
            }

            return iterCount;
        }

        private void FeelArea(Complex[,] Area, Ranges Diap)
        {
            double deltaRe = Diap.maxRe - Diap.minRe;
            double deltaIm = Diap.maxIm - Diap.minIm;

            int ReLength = Area.GetLength(0);
            int ImLength = Area.GetLength(1);

            for (int i = 0; i < ImLength; i++)
            {
                for (int j = 0; j < ReLength; j++)
                {
                    double Re = Diap.minRe + deltaRe * j / ((float)ReLength);
                    double Im = Diap.minIm + deltaIm * i / ((float)ImLength);
                    Area[j, i] = new Complex(Re, Im);
                }
            }
        }

        private void CheckDiap(double r)
        {
            if (workDiap.FramesIsCorrect())
                return;

            if (area.GetLength(1) < area.GetLength(0))
            {
                workDiap.maxIm = r;
                workDiap.minIm = -r;
                float k = ((float)area.GetLength(0)) / area.GetLength(1);
                workDiap.maxRe = r * k;
                workDiap.minRe = -r * k;
            }
            else
            {
                workDiap.maxRe = r;
                workDiap.minRe = -r;
                float k = ((float)area.GetLength(0)) / area.GetLength(1);
                workDiap.maxIm = r * k;
                workDiap.minIm = -r * k;
            }
        }

        private double FindR(Complex c)
        {
            return (1 + Math.Sqrt(1 + 4 * c.Magnitude)) / 2;
        }
        #endregion
    }

    struct Ranges
    {
        public double minRe;
        public double maxRe;
        public double minIm;
        public double maxIm;

        public Ranges(double minRe, double maxRe, double minIm, double maxIm)
        {
            this.minRe = minRe;
            this.maxRe = maxRe;
            this.minIm = minIm;
            this.maxIm = maxIm;
        }

        public bool FramesIsCorrect()
        {
            if (double.IsNaN(maxIm) ||
               double.IsNaN(minIm) ||
               double.IsNaN(maxRe) ||
               double.IsNaN(minRe))
                return false;

            if (maxIm == minIm || maxRe == minRe)
                return false;

            return true;

        }
    }

}
