using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobMLWBOX
{
    public partial class frmSecPassword : Form
    {
        public bool IsAuthorized { get; private set; } = false;

        private readonly string _correctPass;

        // Your static master pass 
        //KIM_07042025
        private const string _staticAdminPass = "vcsi123!@#"; // <-- CHANGE THIS
        public frmSecPassword(string correctPass)
        {
            InitializeComponent();
            _correctPass = correctPass;

            this.AcceptButton = btnEnter;
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            string enteredPass = txtsecpass.Text.Trim();

            if (enteredPass == _correctPass || enteredPass == _staticAdminPass)
            {
                IsAuthorized = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtsecpass.Clear();
                txtsecpass.Focus();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
