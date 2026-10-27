using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public class ErrorLogger
    {
        private static readonly string logFileName = "ErrorLog.txt";
        private static readonly string logFilePath = "C:\\New folder\\" + logFileName;

        public static void LogError(string errorMessage)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging: {ex.Message}");
            }
        }
    }
}
