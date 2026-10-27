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
        //KimConnectionString
        public string connectionString = "Data Source=C:\\Robinson\\RobinsonTemp.db;Version=3;";
        //public string connectionString = "Data Source=RobinsonTemp.db;Version=3;";

        public void log(string filename, int count, string status)
        {
            try
            {
                string date = DateTime.Now.ToString("MM-dd-yyyy");
                string hour = DateTime.Now.ToString("HH:mm");

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    // Check if the record already exists
                    string checkQuery = "SELECT COUNT(*) FROM batchfile WHERE FileName = @FileName";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@FileName", filename);
                        long exists = (long)checkCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            // Update existing record's Status
                            string updateQuery = "UPDATE batchfile SET Status = @Status, TransDate = @Date, TransTime = @Hour WHERE FileName = @FileName";
                            using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@Status", status);
                                updateCmd.Parameters.AddWithValue("@Date", date);
                                updateCmd.Parameters.AddWithValue("@Hour", hour);
                                updateCmd.Parameters.AddWithValue("@FileName", filename);

                                int rowsUpdated = updateCmd.ExecuteNonQuery();
                                Console.WriteLine($"{rowsUpdated} row(s) updated.");
                            }
                        }
                        else
                        {
                            // Insert new record
                            string insertQuery = "INSERT INTO batchfile (FileName, Count, TransDate, TransTime, Status) VALUES (@FileName, @Count, @Date, @Hour, @Status)";
                            using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@FileName", filename);
                                insertCmd.Parameters.AddWithValue("@Count", count);
                                insertCmd.Parameters.AddWithValue("@Date", date);
                                insertCmd.Parameters.AddWithValue("@Hour", hour);
                                insertCmd.Parameters.AddWithValue("@Status", status);

                                int rowsInserted = insertCmd.ExecuteNonQuery();
                                Console.WriteLine($"{rowsInserted} row(s) inserted.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving:\n{ex.Message}", "Contact Vendor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ErrorLogger.LogError($"Error Saving:\n{ex.Message} || {ex.StackTrace}");
                return;
            }
        }

        //public void log(string filename, int count, string status)
        //{
        //    //MessageBox.Show($"Filename: {filename}\nCount: {count}\nStatus: {status}\nConn: {connectionString}");
        //    try
        //    {
        //        string date = DateTime.Now.ToString("MM-dd-yyyy");
        //        string hour = DateTime.Now.ToString("HH:mm");

        //        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        //        {
        //            conn.Open();

        //            string query = "INSERT INTO batchfile (FileName, Count, TransDate, TransTime, Status) VALUES (@FileName, @Count, @Date, @Hour, @Status)";

        //            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@FileName", filename);
        //                cmd.Parameters.AddWithValue("@Count", count);
        //                cmd.Parameters.AddWithValue("@Date", date);
        //                cmd.Parameters.AddWithValue("@Hour", hour);
        //                cmd.Parameters.AddWithValue("@Status", status);

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                Console.WriteLine($"{rowsAffected} row(s) inserted.");
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show($"Error Saving:\n{ex.Message}", "Contact Vendor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        ErrorLogger.LogError($"Error Saving:\n{ex.Message} || {ex.StackTrace}");
        //        return;
        //    }
        //}
    }
}
