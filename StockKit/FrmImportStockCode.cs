using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aspose.Cells;
using AhDung.WinForm;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace StockKit
{
    public partial class FrmImportStockCode : Form
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);
        public FrmImportStockCode()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
        }
        DataTable dt = new DataTable();
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Excel files|*.xls";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtExcelFileName.Text = openFileDialog1.FileName;
                Workbook book = new Workbook(openFileDialog1.FileName);
                Worksheet sheet = book.Worksheets[0];
                var cells = sheet.Cells;
                //获取excel中的数据保存到一个datatable中
                dt = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, true);
                dgv.DataSource = new BindingSource(dt, string.Empty);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            btnOK.Enabled = false;
            Task.Run(() => {
                int errCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        var stockCode = dr[0].ToString();
                        var code = stockCode.Substring(2);
                        var exchange = stockCode.Substring(0, 2);
                        var name = dr[1].ToString(); // 股票名称
                        var price = Convert.ToDecimal(dr[2]); // 最新价格
                        var jinKai = Convert.ToDecimal(dr[9]); // 今开
                        var zuoShou = Convert.ToDecimal(dr[10]); // 昨收
                        var zuiGao = Convert.ToDecimal(dr[11]); // 最高
                        var zuiDi = Convert.ToDecimal(dr[12]); // 最低

                        Database.SqlServer.Instance.ExcuteSQL(new[] {
                                $"DELETE FROM CODES WHERE CODE = '{code}'",
                                $"INSERT INTO CODES (code, exchange) VALUES('{code}', '{exchange}')",
                                $"DELETE FROM StockStatus WHERE CODE = '{code}'",
                                $"INSERT INTO StockStatus(code, exchange, name, price, zuoShou, jinKai, zuiGao, zuiDi, timestamp) values('{code}', '{exchange}', '{name}', {price}, {zuoShou}, {jinKai}, {zuiGao}, {zuiDi}, '{DateTime.Now.ToString()}')"
                            });
                    }
                    catch (Exception ex)
                    {
                        errCount++;
                        OutputDebugString($"Exception:{ex.Message}, {ex.StackTrace}");
                    }
                }
                Invoke(new Action(()=> {
                    MsgBox.ShowInfo(
                        message: "执行完成", // 消息摘要
                        attach: $"共导入数据{dt.Rows.Count}条。其中{errCount}条数据出错。",  // 详细信息
                        caption: "消息", // 窗口标题
                        expand: false, // 是否展开详细信息
                        buttonText: "OK" // 确定按钮文字
                    );
                    Close();
                    DialogResult = DialogResult.OK;
                }));
            });
        }

        private void linkLabelDataSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://stockapp.finance.qq.com/mstats/#mod=list&id=sha&module=SS&type=rankash");
        }
    }
}
