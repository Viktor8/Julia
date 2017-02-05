using System;
using System.IO;
namespace Julia
{
    class Logger
    {
        private static readonly object _syncObject = new object();

        public static void Log(string logMessage)
        {
            // only one thread can own this lock, so other threads
            // entering this method will wait here until lock is
            // available.
            lock (_syncObject)
            {
                TextWriter w = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\log.txt");

                w.Write("{0} {1} \t", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
               // w.WriteLine("  :");
                w.WriteLine(logMessage);
               // w.WriteLine("-------------------------------");
                // Update the underlying file.
                w.Flush();
                w.Close();
            }
        }
    }
}
