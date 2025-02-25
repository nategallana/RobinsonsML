using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX
{
    static class Program
    {
        public static string DatabaseFile { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DateTime newDate = DateTime.Now;
            GlobalVar.StartDate = newDate;
            Application.Run(new frmMallLinking());
        }
    }
}
