using RobinsonMallLinkingWBOX.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public class SettingsWriter
    {
        private string _directory = string.Empty;
        private bool _isSecured = false;

        public SettingsWriter(string directory, bool secure)
        {
            _directory = directory;
            _isSecured = secure;
            if (!String.IsNullOrEmpty(_directory))
            {
                if (!Directory.Exists(_directory))
                {
                    Directory.CreateDirectory(_directory);
                }
            }
        }

        public bool Save<T>(T setting, string fileName)
        {
            try
            {
                string serializeConfig = EntityConversion.Serialize(setting);
                string settingFilename = Path.Combine(_directory, fileName);
                if (File.Exists(settingFilename))
                {
                    try
                    {
                        File.Delete(settingFilename);
                    }
                    catch { }
                }
                using (StreamWriter writer = new StreamWriter(settingFilename, false))
                {
                    if (_isSecured)
                    {
                        writer.Write(Cipher.Encrypt(serializeConfig));
                    }
                    else
                    {
                        writer.Write(serializeConfig);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public T Read<T>(string fileName)
        {
            T setting = default(T);
            string settingFilename = Path.Combine(_directory, fileName);
            if (File.Exists(settingFilename))
            {
                string content = File.ReadAllText(settingFilename);
                if (!string.IsNullOrEmpty(content))
                {
                    if (_isSecured)
                    {
                        string unSecuredContent = Cipher.Decrypt(content);
                        if (!string.IsNullOrEmpty(unSecuredContent))
                        {
                            setting = EntityConversion.Deserialize<T>(unSecuredContent);
                        }
                    }
                    else
                    {
                        setting = EntityConversion.Deserialize<T>(content);
                    }
                }
            }
            return setting;
        }
    }
}
