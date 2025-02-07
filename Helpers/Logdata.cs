using RobinsonMallLinkingWBOX.Constants;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public class Logdata
    {
        public string connectionString = "Data Source=RobinsonTemp.db;Version=3;";

        public void log(string filename, int count, string status)
        {
           try
            {
                string date = DateTime.Now.ToString("MM-dd-yyyy");
                string hour = DateTime.Now.ToString("HH:MM");

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO BatchFile (FileName, Count, TransDate, TransTime, Status) VALUES (@FileName, @Count, @Date, @Hour, @Status)";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FileName", filename);
                        cmd.Parameters.AddWithValue("@Count", count);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Hour", hour);
                        cmd.Parameters.AddWithValue("@Status", status);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} row(s) inserted.");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error Saving:\n{ex.Message}", "Contact Vendor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ErrorLogger.LogError($"Error Saving:\n{ex.Message} || {ex.StackTrace}");
                return;
            }
        }
    }
}
