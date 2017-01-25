using System;
using System.Drawing;


namespace Julia
{
    static class HSV_Color
    {
        public static Color FromAHSV(this Color Clr, byte A, float H, float S, float V)
        {

            int hi = Convert.ToInt32(Math.Floor(H / 60)) % 6;
            double f = H / 60 - Math.Floor(H / 60);

            V = V * 255;
            int v = Convert.ToInt32(V);
            int p = Convert.ToInt32(V * (1 - S));
            int q = Convert.ToInt32(V * (1 - f * S));
            int t = Convert.ToInt32(V * (1 - (1 - f) * S));

            if (hi == 0)
                return Color.FromArgb(A, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(A, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(A, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(A, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(A, t, p, v);
            else
                return Color.FromArgb(A, v, p, q);
        }

        public static float GetSaturationHSV(this Color Clr)
        {
            int max = Math.Max(Clr.R, Math.Max(Clr.G, Clr.B));
            int min = Math.Min(Clr.R, Math.Min(Clr.G, Clr.B));


            return (float)((max == 0) ? 0 : 1d - (1d * min / max));
            
        }

        public static float GetValueHSV(this Color Clr)
        {
            int max = Math.Max(Clr.R, Math.Max(Clr.G, Clr.B));
            return max / 255f; 
        }
    }
}
