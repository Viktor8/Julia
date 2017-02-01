using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Julia
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //System.Drawing.Bitmap bm = new System.Drawing.Bitmap(2731, 2731, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //for (int i = 0; i < 1; i++)
            //{
            //    var bmd = bm.LockBits(new System.Drawing.Rectangle(0, 0, 2731, 2731),
            //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bm.PixelFormat);
            //    unsafe
            //    {
            //        Julia.Calculate((byte*)bmd.Scan0.ToInt32(), 2731, Area.GetDefault().ReMin, Area.GetDefault().ReSize,
            //            Area.GetDefault().ImMin, Area.GetDefault().ImSize, 1000);
            //    }
            //    bm.UnlockBits(bmd);
            //    //GC.Collect();
            //}
            //int t = (int)sw.ElapsedMilliseconds;

            int[] arr = new int[10];
        
         

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
