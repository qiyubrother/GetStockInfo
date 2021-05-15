using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Database;
using AhDung.WinForm;
namespace StockKit
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            Icon = Properties.Resources.stock;
            var s = File.ReadAllText("config.json");
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
            Database.SqlServer.ConnectionString = jo["ConnectionString"].ToString();
        }

        private void btnEditConnectionString_Click(object sender, EventArgs e)
        {
            new FrmEditConnectString().ShowDialog();
        }

        private void btnUpdateStockCodePool_Click(object sender, EventArgs e)
        {
            //Hide();
            //new FrmGetStockCode().ShowDialog();
            //Show();

            new FrmImportStockCode().ShowDialog();
        }

        private void btnGetStockIndustry_Click(object sender, EventArgs e)
        {
            try
            {
                var p = Process.Start("GetIndustry.exe");
                //this.Enabled = false;
                //while (!p.HasExited) Application.DoEvents();
                //Enabled = true;

                //new FrmGetTodayStockInfo().ShowDialog();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(
                    message: "消息内容", // 消息摘要
                    attach: $"{ex.Message}, {ex.StackTrace}",  // 详细信息
                    caption: "消息", // 窗口标题
                    expand: false, // 是否展开详细信息
                    buttonText: "OK" // 确定按钮文字
                    );
            }
        }

        private void btnStockPool_Click(object sender, EventArgs e)
        {
            //Hide();
            new FrmStockPoolMaster().ShowDialog();
            //Show();
        }

        private void btnHistoryData_Click(object sender, EventArgs e)
        {
            //Hide();
            new FrmHistoryData().ShowDialog();
            //Show();
        }

        private void btnRiskNDay_Click(object sender, EventArgs e)
        {
            Task.Run(()=> {
                List<RowData> lst = new List<RowData>();
                List<Codes> codeList = new List<Codes>();

                var riskDays = 3;
                var frm = new FrmSetRiskDay();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    riskDays = frm.RiskDays;
                }

                var outPath = Path.Combine(Application.StartupPath, "Report");
                if (!Directory.Exists(outPath))
                {
                    Directory.CreateDirectory(outPath);
                }
                var fileName = Path.Combine(outPath, $"RiskNDaysStock[{DateTime.Now.ToString("yyyyMMddhhmmss")}].html");

                var swReport = new StreamWriter(fileName, false, Encoding.UTF8);
                var reportDateTime = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}-{DateTime.Now.Hour.ToString().PadLeft(2, '0')}{DateTime.Now.Minute.ToString().PadLeft(2, '0')}";
                swReport.WriteLine($"<!DOCTYPE html>");
                swReport.WriteLine($"<html>");
                swReport.WriteLine($"<head>");
                swReport.WriteLine($"<meta charset=\"UTF-8\">");
                swReport.WriteLine($"<link rel='stylesheet' href='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/css/bootstrap.min.css'>  ");
                swReport.WriteLine($"<script src='https://cdn.staticfile.org/jquery/2.1.1/jquery.min.js'></script>");
                swReport.WriteLine($"<script src='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/js/bootstrap.min.js'></script>");
                swReport.WriteLine($"<title>Rising Stocks Report</title>");
                swReport.WriteLine($"</head>");
                swReport.WriteLine($"<body>");

                var sz = "sh000001"; // 上证指数
                var sc = "sz399001"; // 深证成指
                var cy = "sz399006"; // 创业板指
                var kc50 = "sh000688"; // 科创50指数
                var risingDetailBuilder = new StringBuilder();
                risingDetailBuilder.Append($"<table class='table table-striped table-bordered table-hover'>");
                risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sz}.html'>上证指数</a><br>{sz}</td><td><image src='data:image/png;base64,{StockHelper.GetFenShiImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetRiKXianImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetZhouKXianImageBase64(sz)}' /></td></tr>");
                risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sc}.html'>深证成指</a><br>{sc}</td><td><image src='data:image/png;base64,{StockHelper.GetFenShiImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetRiKXianImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetZhouKXianImageBase64(sc)}' /></td></tr>");
                risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{cy}.html'>创业板指</a><br>{cy}</td><td><image src='data:image/png;base64,{StockHelper.GetFenShiImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetRiKXianImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetZhouKXianImageBase64(cy)}' /></td></tr>");
                risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{kc50}.html'>科创50指数</a><br>{kc50}</td><td><image src='data:image/png;base64,{StockHelper.GetFenShiImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetRiKXianImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetZhouKXianImageBase64(kc50)}' /></td></tr>");
                int posYang = 0;

                var dt = SqlServer.Instance.GetDataTable($"SELECT a.code, a.Exchange, b.name FROM CODES as a left join StockStatus as b on a.code = b.code");
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add(new Codes { Code = dr["Code"].ToString(), Exchange = dr["Exchange"].ToString(), Name = dr["Name"].ToString() });
                }
                foreach (var code in codeList)
                {
                    var name = code.Name;
                    if (name.StartsWith("ST") ||
                        name.StartsWith("*ST") ||
                        name.StartsWith("N") ||
                        name.Contains("退")
                        )
                    {
                        continue;
                    }
                    var hisDateStart = DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd");
                    var hisDateEnd = DateTime.Now.ToString("yyyy-MM-dd");

                    var dtR = SqlServer.Instance.GetDataTable($"select * from StockHistory where Code='{code.Code}'");
                    var rank = string.Empty;

                    #region 计算连续阳线天数
                    if (dtR.Rows.Count >= riskDays)
                    {
                        var cnt = 0;
                        foreach (DataRow dr in dtR.Rows)
                        {
                            if ((decimal)dr["ShouPan"] > (decimal)dr["JinKai"])
                            {
                                cnt++;
                            }
                            else
                            {
                                cnt = 0;
                                break;
                            }
                        }
                        if (cnt > 0 && cnt == dtR.Rows.Count) // 所有天都是阳线收盘
                        {
                            ++posYang;
                            var industry = string.Empty;
                            var industryHref = "#";

                            var __dt = SqlServer.Instance.GetDataTable($"select * from StockIndustry where Code='{code.Code}'");
                            if (__dt.Rows.Count > 0)
                            {
                                industry = __dt.Rows[0]["Industry"].ToString();
                                industryHref = __dt.Rows[0]["Href"].ToString();
                            }
                            risingDetailBuilder.Append($"<tr><td style='color:red;'>R{riskDays}-{posYang}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{StockHelper.GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{StockHelper.GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                        }
                    }
                    #endregion
                }

                risingDetailBuilder.Append($"</table>");
                swReport.WriteLine(risingDetailBuilder);
                swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
                swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
                swReport.WriteLine($"<h5>软件版本：{2.1}</h5>");
                swReport.WriteLine($"</div>");
                swReport.WriteLine($"</body>");
                swReport.WriteLine($"<html>");
                swReport.Close();

                Invoke(new Action(()=> {
                    MsgBox.ShowInfo(
                        message: "OK", // 消息摘要
                        attach: fileName,  // 详细信息
                        caption: "消息", // 窗口标题
                        expand: false, // 是否展开详细信息
                        buttonText: "OK" // 确定按钮文字
                    );
                }));
            });
        }
    }
}
