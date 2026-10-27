using RobinsonMallLinkingWBOX.Models;
using RobMLWBOX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX
{
    static class Program
    {
        public static string DatabaseFile { get; set; }
        private static Mutex mutex = new Mutex(true, "RobMLWBOXUniqueMutex");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]


        static void Main()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var existingProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                                         .FirstOrDefault(p => p.Id != currentProcess.Id);

            if (existingProcess != null)
            {
                // Close the existing instance
                existingProcess.Kill();
                existingProcess.WaitForExit(); // Ensure it's completely closed

                // Start a fresh instance
                Process.Start(currentProcess.MainModule.FileName);
                return; // Exit this instance to prevent two running
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DateTime newDate = DateTime.Now;
            GlobalVar.StartDate = newDate;
            Application.Run(new frmMallLinking());

            mutex.ReleaseMutex();
        }
    }
}