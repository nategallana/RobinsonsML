using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Security
{
    public class Xor
    {
        public static string Reverse(string input, string key)
        {
            string result = null;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (StringReader stringReader = new StringReader(input))
                {
                    int length = key.Length;
                    int num = 0;
                    int num2;
                    while ((num2 = stringReader.Read()) > -1)
                    {
                        stringWriter.Write(Convert.ToChar(num2 ^ (int)key[num % length]));
                        num++;
                    }
                }
                result = stringWriter.ToString();
            }
            return result;
        }
    }
}
