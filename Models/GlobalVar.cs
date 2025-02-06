using RobinsonMallLinkingWBOX.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX.Models
{
    public class GlobalVar
    {
        public static SettingsWriter _settings = new SettingsWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Application.StartupPath, "Settings"), false);
        public static Configuration _systemConfig = new Configuration();

        static string XML, host, uname, pword, dir, tname, terno, tempmdb, ssh = null;

        public static void InitializeConfiguration()
        {
            _systemConfig = _settings.Read<Configuration>("Configuration");
            if (_systemConfig != null)
            {
                XML = _systemConfig.XMLPath.Trim().ToString();
                host = _systemConfig.Host.Trim().ToString();
                uname = _systemConfig.Username.Trim().ToString();
                pword = _systemConfig.Password.Trim().ToString();
                dir = _systemConfig.Directory.Trim().ToString();
                tname = _systemConfig.TenantName.Trim().ToString();
                terno = _systemConfig.TerminalNo.Trim().ToString();
                tempmdb = _systemConfig.TempMDB.Trim().ToString();
                ssh = _systemConfig.SSHKey.Trim().ToString();
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
        public static string SSH { get => ssh; set => ssh = value.ToString(); }
        public static bool SuccessSending { get; set; }
        public static bool SuccessSending2 { get; set; }
        public static bool ErrorSending { get; set; }
    }
}
