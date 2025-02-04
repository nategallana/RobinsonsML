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

        static string XML, host, uname, pword, dir, tname, terno, tempmdb = null;

        public Settings()
        {
            InitializeComponent();
            _settings = new SettingsWriter(Path.Combine(Application.StartupPath, "Settings"), false);
            InitializeConfiguration();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _settings.Save<Configuration>(SystemConfiguration, "Configuration");
            MessageBox.Show("Configuration has been saved successfully! \n\nPlease restart the application.", "Notice");
            Application.Exit();
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
                    TempMDB = txtTempDB.Text.Trim().ToString()
                };
            }
        }

        public static string XMLPath { get => XML; set => XML = value.ToString(); }


        public static string HOST { get => host; set => host = value.ToString(); }
        public static string USERNAME { get => uname; set => uname = value.ToString(); }
        public static string PASSWORD { get => pword; set => pword = value.ToString(); }
        public static string DIRECTORY { get => dir; set => dir = value.ToString(); }
        public static string TENANTNAME { get => tname; set => tname = value.ToString(); }
        public static string TERMINALNO { get => terno; set => terno = value.ToString(); }
        public static string TEMPMDB { get => tempmdb; set => tempmdb = value.ToString(); } 
    }
}
