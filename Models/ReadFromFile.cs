using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX
{
    public class ReadFromFile
    {
        public static void ReqReadDataFromFile()
        {
            //TransactionData trandata = new TransactionData();
            //string directory = AppDomain.CurrentDomain.BaseDirectory;
            Settings settings = new Settings();
            string filepath = Settings.XMLPath;

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath);

                if (lines.Length == 0)
                {
                    HandleEmptyFile();
                }
                else
                {
                    foreach (string line in lines)
                    {
                        //trandata.filepath = line;
                        string path = line;
                        //TransactionData.Reqfilepath = path;
                        //BackupXML(xml);
                        MessageBox.Show("File  exist at: " + filepath, "File Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //FilePath frm = new FilePath();
                        //frm.Hide();
                    }
                }
            }
            else
            {
                //Form1 _frm = new Form1();
                //_frm.Close();
                MessageBox.Show("File does not exist at: " + filepath, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //FilePath frm = new FilePath();
                //frm.Show();


            }
        }

        public static void ResReadDataFromFile()
        {
            //TransactionData trandata = new TransactionData();
            //string directory = AppDomain.CurrentDomain.BaseDirectory;

            string filepath = Settings.XMLPath;

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath);

                if (lines.Length == 0)
                {
                    HandleEmptyFile();
                }
                else
                {
                    foreach (string line in lines)
                    {
                        //trandata.filepath = line;
                        //string path = line;
                        //TransactionData.Resfilepath = path;
                        MessageBox.Show("File  exist at: " + filepath, "File Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //FilePath frm = new FilePath();
                        //frm.Hide();
                    }
                }
            }
            else
            {
                //Form1 _frm = new Form1();
                //_frm.Close();
                MessageBox.Show("File does not exist at: " + filepath, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //FilePath frm = new FilePath();
                //frm.Show();
            }
        }

        public static void HandleEmptyFile()
        {
            MessageBox.Show("The File is Empty. Please input the file path", "Empty File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //FilePath _frm = new FilePath();
            //_frm.Show();
        }
    }
}
