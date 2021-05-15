using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using AhDung.WinForm;
namespace StockKit
{
    public partial class FrmHistoryData : Form
    {
        public FrmHistoryData()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
        }

        private void FrmHistoryData_Load(object sender, EventArgs e)
        {
            txtStartDate.Text = $"{DateTime.Now.AddDays(-30).ToString("yyyyMMdd")}";
            txtEndDate.Text = $"{DateTime.Now.ToString("yyyyMMdd")}";
        }
        private void btnGetData_Click(object sender, EventArgs e)
        {
            Task.Run(()=>
            {
                List<Codes> codeList = new List<Codes>();
                var startDate = txtStartDate.Text;
                var endDate = txtEndDate.Text;
                var dt = SqlServer.Instance.GetDataTable($"SELECT * FROM CODES");
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add(new Codes { Code = dr["Code"].ToString(), Exchange = dr["Exchange"].ToString() });
                }
                SqlServer.Instance.ExcuteSQL($"delete from StockHistory");

                foreach (var stockCode in codeList)
                {
                    var q = $"https://q.stock.sohu.com/hisHq?code=cn_{stockCode.Code}&start={startDate}&end={endDate}&stat=1&order=D&period=d&callback=historySearchHandler&rt=jsonp";
                    var ___s = StockHelper.GetHtmltxt(q);
                    var jsonData = ___s.Replace("historySearchHandler(", string.Empty).Replace("\n", string.Empty).TrimEnd(')');
                    if (jsonData == "{}")
                    {
                        continue;
                    }
                    var jArr = JsonConvert.DeserializeObject(jsonData) as JArray;
                    var status = jArr[0]["status"].ToString();
                    if (status != "0")
                    {
                        continue;
                    }
                    var _arrJsonData = jArr[0]["hq"] as JArray;
                    foreach (var data in _arrJsonData)
                    {
                        var row = new RowData();
                        row.Code = stockCode.Code;
                        row.Exchange = stockCode.Exchange;
                        row.HisDate = Convert.ToDateTime(data[0], new DateTimeFormatInfo { ShortDatePattern = "yyyy-MM-dd" });
                        decimal.TryParse(data[1].ToString(), out decimal jinKai); // 今开
                        decimal.TryParse(data[2].ToString(), out decimal shouPan); // 收盘
                        decimal.TryParse(data[3].ToString(), out decimal shangZhangJinE); // 上涨金额
                        decimal.TryParse(data[4].ToString().TrimEnd('%'), out decimal shangZhangFuDu); // 上涨幅度
                        decimal.TryParse(data[5].ToString(), out decimal zuiDi); // 最低
                        decimal.TryParse(data[6].ToString(), out decimal zuiGao); // 最高
                        decimal.TryParse(data[7].ToString(), out decimal chengJiaoLiang); // 成交量
                        decimal.TryParse(data[8].ToString(), out decimal chengJiaoE); // 成交额
                        decimal.TryParse(data[9].ToString().TrimEnd('%'), out decimal huanShouLv); // 换手率
                        row.JinKai = jinKai;
                        row.ZuiGao = zuiGao;
                        row.ShangZhangJinE = shangZhangJinE;
                        row.ShangZhangFuDu = shangZhangFuDu;
                        row.ZuiDi = zuiDi;
                        row.ShouPan = shouPan;
                        row.ChengJiaoLiang = chengJiaoLiang;
                        row.ChengJiaoE = chengJiaoE;
                        row.HuanShouLv = huanShouLv;

                        var cmd = new SqlCommand($"INSERT INTO StockHistory(Code, Exchange, HisDate, JinKai, ZuiGao, ShangZhangJinE, ShangZhangFuDu, ZuiDi, ShouPan, ChengJiaoLiang, ChengJiaoE, HuanShouLv) VALUES (@Code, @Exchange, @HisDate, @JinKai, @ZuiGao, @ShangZhangJinE, @ShangZhangFuDu, @ZuiDi, @ShouPan, @ChengJiaoLiang, @ChengJiaoE, @HuanShouLv)");
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter("@Code", row.Code));
                        cmd.Parameters.Add(new SqlParameter("@Exchange", row.Exchange));
                        cmd.Parameters.Add(new SqlParameter("@HisDate", row.HisDate));
                        cmd.Parameters.Add(new SqlParameter("@JinKai", row.JinKai));
                        cmd.Parameters.Add(new SqlParameter("@ZuiGao", row.ZuiGao));
                        cmd.Parameters.Add(new SqlParameter("@ShangZhangJinE", row.ShangZhangJinE));
                        cmd.Parameters.Add(new SqlParameter("@ShangZhangFuDu", row.ShangZhangFuDu));
                        cmd.Parameters.Add(new SqlParameter("@ZuiDi", row.ZuiDi));
                        cmd.Parameters.Add(new SqlParameter("@ShouPan", row.ShouPan));
                        cmd.Parameters.Add(new SqlParameter("@ChengJiaoLiang", row.ChengJiaoLiang));
                        cmd.Parameters.Add(new SqlParameter("@ChengJiaoE", row.ChengJiaoE));
                        cmd.Parameters.Add(new SqlParameter("@HuanShouLv", row.HuanShouLv));
                        SqlServer.Instance.ExcuteSQL(new[] { cmd });
                    }
                }
                Invoke(new Action(()=> {
                    MsgBox.ShowInfo(
                        message: "执行成功。", // 消息摘要
                        attach: "",  // 详细信息
                        caption: "消息", // 窗口标题
                        expand: false, // 是否展开详细信息
                        buttonText: "OK" // 确定按钮文字
                        );
                }));

            });
        }
    }

    public class RowData
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime HisDate { get; set; }
        /// <summary>
        /// 今开
        /// </summary>
        public decimal JinKai { get; set; }
        /// <summary>
        /// 最高
        /// </summary>
        public decimal ZuiGao { get; set; }
        /// <summary>
        /// 上涨金额
        /// </summary>
        public decimal ShangZhangJinE { get; set; }
        /// <summary>
        /// 上涨幅度
        /// </summary>
        public decimal ShangZhangFuDu { get; set; }
        /// <summary>
        /// 最低
        /// </summary>
        public decimal ZuiDi { get; set; }
        /// <summary>
        /// 收盘
        /// </summary>
        public decimal ShouPan { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public decimal ChengJiaoLiang { get; set; }
        /// <summary>
        /// 成交额
        /// </summary>
        public decimal ChengJiaoE { get; set; }
        /// <summary>
        /// 换手率
        /// </summary>
        public decimal HuanShouLv { get; set; }
    }
    sealed class Codes
    {
        public string Exchange { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
