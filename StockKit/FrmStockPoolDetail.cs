using Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AhDung.WinForm;

namespace StockKit
{
    public partial class FrmStockPoolDetail : Form
    {
        DataTable dt = new DataTable();
        public FrmStockPoolDetail(string schemaId, string schemaName)
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
            txtSchemaId.Text = schemaId;
            txtSchemaName.Text = schemaName;
            dgv.Focus();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Reload();
        }
        private void Reload()
        {
            dgv.DataSource = null;
            txtCode.DataBindings.Clear();
            txtName.DataBindings.Clear();
            dt = SqlServer.Instance.GetDataTable($"SELECT a.code, b.name FROM SchemaDetail as a left join stockstatus as b on a.code = b.code where a.SchemaId = '{txtSchemaId.Text}' order by code");
            var bs = new BindingSource(dt, string.Empty);
            dgv.DataSource = bs;
            txtCode.DataBindings.Add("Text", bs, "Code");
            txtName.DataBindings.Add("Text", bs, "Name");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var exchange = string.Empty;
            var t = SqlServer.Instance.GetDataTable($"SELECT * FROM [stockstatus] where code = '{txtNewCode.Text}'");
            if (t.Rows.Count > 0)
            {
                exchange = t.Rows[0]["Exchange"].ToString();
            }
            else
            {
                throw new Exception("stockstatus error.");
            }
            SqlServer.Instance.ExcuteSQL($"insert into [SchemaDetail](schemaId, code, exchange) values({txtSchemaId.Text}, '{txtNewCode.Text}', '{exchange}')");
            Reload();
            txtNewCode.Clear();
            txtNewName.Clear();
        }

        private void txtNewCode_TextChanged(object sender, EventArgs e)
        {
            if (txtNewCode.TextLength == 6)
            {
                var t = SqlServer.Instance.GetDataTable($"SELECT * FROM [stockstatus] where code = '{txtNewCode.Text}'");
                if (t.Rows.Count > 0)
                {
                    txtNewName.Text = t.Rows[0]["Name"].ToString();
                }
                else
                {
                    txtNewName.Text = string.Empty;
                }
            }
            else
            {
                txtNewName.Text = string.Empty;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MsgBox.ShowQuestion(
                message: $"确认删除“{txtName.Text}”吗？", // 消息摘要
                attach: "删除后将无法恢复。",  // 详细信息
                caption: "消息", // 窗口标题
                buttons: MessageBoxButtons.YesNo, // 按钮样式
                defaultButton: MessageBoxDefaultButton.Button1, // 默认按钮
                expand: false, // 是否展开详细
                buttonsText: new[] { "Yes", "No" } // 按钮文字
                ))
            {
                SqlServer.Instance.ExcuteSQL($"delete from [SchemaDetail] where schemaId = '{txtSchemaId.Text}' and code = '{txtCode.Text}'");
                Reload();
            }
        }
    }
}
