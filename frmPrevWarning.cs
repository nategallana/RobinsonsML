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
    public partial class frmPrevWarning : Form
    {
        public frmPrevWarning()
        {
            InitializeComponent();
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close(); // Closes the form when the OK button is clicked
        }

        private void frmPrevWarning_Load(object sender, EventArgs e)
        {

        }
    }
}
