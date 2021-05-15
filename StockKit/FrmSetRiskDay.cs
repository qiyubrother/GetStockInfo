using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockKit
{
    public partial class FrmSetRiskDay : Form
    {
        public FrmSetRiskDay()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
        }
        public int RiskDays { get; private set; }
        private void btnOK_Click(object sender, EventArgs e)
        {
            RiskDays = (int)numericUpDown1.Value;
            Close();
            DialogResult = DialogResult.OK;
        }
    }
}
