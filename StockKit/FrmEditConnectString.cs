using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Database;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace StockKit
{
    public partial class FrmEditConnectString : Form
    {
        public FrmEditConnectString()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
            var s = File.ReadAllText("config.json");
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
            txtConnectionString.Text = jo["ConnectionString"].ToString();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            SqlServer.Test(txtConnectionString.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var jo = new JObject();
            jo["ConnectionString"] = txtConnectionString.Text;
            Database.SqlServer.ConnectionString = txtConnectionString.Text;
            var s = jo.ToString();
            File.WriteAllText("config.json", s);
        }
    }
}
