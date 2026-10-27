using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Models
{
    public class Configuration
    {
        public string XMLPath { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Directory { get; set; }
        public string TenantName { get; set; }
        public string TerminalNo { get; set; }
        public string TempMDB { get; set; }
        public string SSHKey { get; set; }

        //KIM_07042025
        public string SecPass { get; set; } // << your textbox value
        public string StartTime { get; set; } // << DateTimePicker Time only
        public string EndTime { get; set; }  // << DateTimePicker Time only
    }
}
