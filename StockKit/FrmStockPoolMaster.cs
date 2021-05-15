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
using AhDung.WinForm;
using System.IO;
using System.Diagnostics;

namespace StockKit
{
    public partial class FrmStockPoolMaster : Form
    {
        DataTable dt = new DataTable();
        public FrmStockPoolMaster()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Reload();
        }

        private void Reload()
        {
            dgv.DataSource = null;
            txtId.DataBindings.Clear();
            txtName.DataBindings.Clear();
            dt = SqlServer.Instance.GetDataTable($"SELECT * FROM [SCHEMA] order by schemaId");
            var bs = new BindingSource(dt, string.Empty);
            dgv.DataSource = bs;
            txtId.DataBindings.Add("Text", bs, "SchemaId");
            txtName.DataBindings.Add("Text", bs, "SchemaName");
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            SqlServer.Instance.ExcuteSQL($"update [SCHEMA] set schemaName = '{txtName.Text}' where schemaId = '{txtId.Text}'");

            Reload();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var newSchemaId = 1;
            var tbl = SqlServer.Instance.GetDataTable($"select max(schemaId) from [SCHEMA]");
            if (tbl.Rows.Count > 0)
            {
                newSchemaId = Convert.ToInt32(tbl.Rows[0][0]) + 1;
            }
            SqlServer.Instance.ExcuteSQL($"insert into [SCHEMA](schemaId, schemaName) values({newSchemaId}, '{txtNewSchemaName.Text}')");
            Reload();
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
                SqlServer.Instance.ExcuteSQL($"delete from [SCHEMA] where schemaId = '{txtId.Text}'");
                SqlServer.Instance.ExcuteSQL($"delete from [SchemaDetail] where schemaId = '{txtId.Text}'");
                Reload();
            }
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            //Hide();
            var frm = new FrmStockPoolDetail(txtId.Text, txtName.Text);
            frm.ShowDialog();
            //Show();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            var rptName = $"Report[{txtName.Text}][{DateTime.Now.ToString("yyyy-MM-dd HHmmss")}].html";
            var outPath = Path.Combine(Application.StartupPath, "Report");
            var outFileName = Path.Combine(outPath, rptName);
            Task.Run(()=>
            {
                if (!Directory.Exists(outPath))
                {
                    Directory.CreateDirectory(outPath);
                }
                var fileName = StockHelper.MakeReport(txtId.Text, outFileName);
                if (DialogResult.Yes == MsgBox.ShowQuestion(
                    message: "消息内容", // 消息摘要
                    attach: "",  // 详细信息
                    caption: "消息", // 窗口标题
                    buttons: MessageBoxButtons.YesNo, // 按钮样式
                    defaultButton: MessageBoxDefaultButton.Button1, // 默认按钮
                    expand: false, // 是否展开详细
                    buttonsText: new[] { "Yes", "No" } // 按钮文字
                    ))
                {
                    try
                    {
                        Process.Start($"{outFileName}");
                    }
                    catch(Exception ex)
                    {
                        ;
                    }
                }
                //MsgBox.ShowInfo(
                //    message: "OK", // 消息摘要
                //    attach: fileName,  // 详细信息
                //    caption: "消息", // 窗口标题
                //    expand: false, // 是否展开详细信息
                //    buttonText: "OK" // 确定按钮文字
                //);
            });
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >=0 && e.ColumnIndex >=0)
            {
                btnDetail.PerformClick();
            }
            
        }
    }
}
