using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Security
{
    public class Cipher
    {
        private static readonly string key = "112823JuliusPOGI!@#";

        public static string Encrypt(string text)
        {
            byte[] inArray = GZipper.Compress(text);
            string input = Convert.ToBase64String(inArray);
            return Xor.Reverse(input, key);
        }

        public static string Decrypt(string decryptedText)
        {
            string s = Xor.Reverse(decryptedText, key);
            if (!string.IsNullOrEmpty(s))
            {
                byte[] input = null;
                try
                {
                    input = Convert.FromBase64String(s);
                }
                catch
                {

                }
                return GZipper.Decompress(input);
            }
            return string.Empty;
        }
    }
}
