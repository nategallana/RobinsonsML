using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinsonMallLinkingWBOX
{
    public partial class FileView : Form
    {
        public FileView()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=RobinsonTemp.db;Version=3;";
        private void FileView_Load(object sender, EventArgs e)
        {
            
            
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM batchfile;";
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgv_Data.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void X_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
