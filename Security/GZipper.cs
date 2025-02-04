using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Security
{
    public class GZipper
    {
        public static byte[] Compress(string input)
        {
            byte[] result = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(input);
                    gZipStream.Write(bytes, 0, bytes.Length);
                }
                result = memoryStream.ToArray();
            }
            return result;
        }

        public static string Decompress(byte[] input)
        {
            string result = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
                {
                    byte[] array = new byte[4096];
                    int count;
                    while ((count = gZipStream.Read(array, 0, array.Length)) > 0)
                    {
                        memoryStream.Write(array, 0, count);
                    }
                }
                result = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return result;
        }
    }
}
