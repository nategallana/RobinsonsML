using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX
{
    public partial class Settings : Form
    {
        public SettingsWriter _settings = null;
        public static Configuration _systemConfig = new Configuration();

        static string XML, host, uname, pword, dir, tname, terno, tempmdb, ssh = null;

        public Settings()
        {
            InitializeComponent();
            _settings = new SettingsWriter(Path.Combine(Application.StartupPath, "Settings"), false);
            InitializeConfiguration();
        }

        //KIM_07042025
        public static void LoadSystemConfig()
        {
            var writer = new SettingsWriter(Path.Combine(Application.StartupPath, "Settings"), false);
            _systemConfig = writer.Read<Configuration>("Configuration");
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            _settings.Save<Configuration>(SystemConfiguration, "Configuration");

            var result = MessageBox.Show(
                "Configuration has been saved successfully!\n\nWould you like to restart the application now?",
                "Notice",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                Application.Restart();
                System.Diagnostics.Process.Start(Application.ExecutablePath);
            }
        }

        public void InitializeConfiguration()
        {
            _systemConfig = _settings.Read<Configuration>("Configuration");
            if (_systemConfig != null)
            {
                txtXML.Text = _systemConfig.XMLPath.Trim().ToString();
                txtHost.Text = _systemConfig.Host.Trim().ToString();
                txtUname.Text = _systemConfig.Username.Trim().ToString();
                txtPword.Text = _systemConfig.Password.Trim().ToString();
                txtDrctry.Text = _systemConfig.Directory.Trim().ToString();
                txtTenantName.Text = _systemConfig.TenantName.Trim().ToString();
                txtTerminal.Text = _systemConfig.TerminalNo.Trim().ToString();
                txtTempDB.Text = _systemConfig.TempMDB.Trim().ToString();
                txtSSHKey.Text = _systemConfig.SSHKey.Trim().ToString();
                txtsecpass.Text = _systemConfig.SecPass?.Trim(); // << your textbox


                // Safely parse, fallback to default time if empty
                TimeSpan startTime = TimeSpan.Zero;
                TimeSpan endTime = TimeSpan.Zero;

                TimeSpan.TryParse(_systemConfig.StartTime, out startTime);
                TimeSpan.TryParse(_systemConfig.EndTime, out endTime);

                dtpstart.Value = DateTime.Today.Add(startTime);
                dtpend.Value = DateTime.Today.Add(endTime);
            }
        }

        public Configuration SystemConfiguration
        {
            get
            {
                return new Configuration()
                {
                    XMLPath = txtXML.Text.Trim().ToString(),
                    Host = txtHost.Text.Trim().ToString(),
                    Username = txtUname.Text.Trim().ToString(),
                    Password = txtPword.Text.Trim().ToString(),
                    Directory = txtDrctry.Text.Trim().ToString(),
                    TenantName = txtTenantName.Text.Trim().ToString(),
                    TerminalNo = txtTerminal.Text.Trim().ToString(),
                    TempMDB = txtTempDB.Text.Trim().ToString(),
                    SSHKey = txtSSHKey.Text.Trim().ToString(),
                    SecPass = txtsecpass.Text.Trim(), // << your textbox
                    StartTime = dtpstart.Value.TimeOfDay.ToString(@"hh\:mm\:ss"),
                    EndTime = dtpend.Value.TimeOfDay.ToString(@"hh\:mm\:ss")
                };
            }
        }

        public static string XMLPath { get => XML; set => XML = value.ToString(); }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        public static string HOST { get => host; set => host = value.ToString(); }
        public static string USERNAME { get => uname; set => uname = value.ToString(); }
        public static string PASSWORD { get => pword; set => pword = value.ToString(); }
        public static string DIRECTORY { get => dir; set => dir = value.ToString(); }
        public static string TENANTNAME { get => tname; set => tname = value.ToString(); }
        public static string TERMINALNO { get => terno; set => terno = value.ToString(); }
        public static string TEMPMDB { get => tempmdb; set => tempmdb = value.ToString(); } 
        public static string SSH { get => ssh; set => ssh = value.ToString(); }

    }
}
