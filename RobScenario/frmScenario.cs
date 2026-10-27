using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace RobScenario
{
    public partial class frmScenario : Form
    {
        private System.Windows.Forms.Timer appCloseTimer;
        private bool isMessageBoxShown = false;

        private readonly string[] targetProcesses = { "wDesktop", "WirelessLink", "wBoxBridge" };

        public frmScenario()
        {
            InitializeComponent();
            this.ShowInTaskbar = false; // Hide from taskbar
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.None; // Removes UI
            this.Load += FrmScenario_Load;

            InitializeAppCloseHandler();
        }

        private void FrmScenario_Load(object sender, EventArgs e)
        {
            this.Hide(); // Make the form completely invisible
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Hide(); // Prevent showing on startup
        }

        private void InitializeAppCloseHandler()
        {
            appCloseTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000 // Check every 2 seconds
            };
            appCloseTimer.Tick += AppCloseTimer_Tick;
            appCloseTimer.Start();
        }

        private void AppCloseTimer_Tick(object sender, EventArgs e)
        {
            if (IsRestrictedTime() && targetProcesses.Any(IsProcessRunning))
            {
                ShowWarningAndCloseApps();
            }
        }

        private bool IsRestrictedTime()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            return now >= new TimeSpan(4, 0, 0) && now < new TimeSpan(9, 0, 0);
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private void ShowWarningAndCloseApps()
        {
            if (isMessageBoxShown) return;

            isMessageBoxShown = true;
            DialogResult result = ShowMessage("Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted", MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                CloseTargetProcesses();
                isMessageBoxShown = false;
            }
        }

        private DialogResult ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            if (this.InvokeRequired)
            {
                return (DialogResult)this.Invoke(new Func<DialogResult>(() =>
                    MessageBox.Show(text, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly)));
            }
            else
            {
                return MessageBox.Show(text, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CloseTargetProcesses()
        {
            List<string> closedApps = new List<string>();
            List<string> failedApps = new List<string>();

            foreach (var procName in targetProcesses)
            {
                var processes = Process.GetProcessesByName(procName);
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                        closedApps.Add(procName);
                    }
                    catch
                    {
                        failedApps.Add(procName);
                    }
                }
            }

            // Optional: Log or display a summary if needed
            if (closedApps.Any() || failedApps.Any())
            {
                string summary = "";

                if (closedApps.Any())
                    summary += "Closed applications:\n- " + string.Join("\n- ", closedApps) + "\n\n";

                if (failedApps.Any())
                    summary += "Failed to close:\n- " + string.Join("\n- ", failedApps);
            }
        }
    }
}
