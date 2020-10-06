using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RisingStocks
{
    class Program
    {
        static string connectionString = string.Empty;
        static string codesTableName = string.Empty;
        static int risingDays = 1;
        static List<RowData> lst = new List<RowData>();
        static List<Codes> codeList = new List<Codes>();
        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var hisPath = Path.Combine(fi.Directory.Parent.Parent.FullName, "history");
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            codesTableName = jo["CodeSource"].ToString();
            risingDays = Convert.ToInt32(jo["RisingDays"].ToString()); // 连续阳线天数
            var swReport = new StreamWriter(Path.Combine(fi.Directory.FullName, $"RiskNDaysStock.html"), false, Encoding.UTF8);
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
            var zx = "sz399005"; // 中小板指
            var kc50 = "sh000688"; // 科创50指数
            var detailBuilder = new StringBuilder();
            detailBuilder.Append($"<table class='table table-striped table-bordered table-hover'>");
            detailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sz}.html'>上证指数</a><br>{sz}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(sz)}' /></td></tr>");
            detailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sc}.html'>深证成指</a><br>{sc}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(sc)}' /></td></tr>");
            detailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{cy}.html'>创业板指</a><br>{cy}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(cy)}' /></td></tr>");
            detailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{zx}.html'>中小板指</a><br>{zx}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(zx)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(zx)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(zx)}' /></td></tr>");
            detailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{kc50}.html'>科创50指数</a><br>{kc50}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(kc50)}' /></td></tr>");
            int pos = 0;
            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"{codesTableName}", conn);
                var dt = new DataTable();
                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add(new Codes { Code = dr["Code"].ToString(), Exchange = dr["Exchange"].ToString() });
                }

                foreach (var code in codeList)
                {
                    var name = GetStockName(code.Exchange + code.Code);
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
                    ada = new SQLiteDataAdapter($"select * from StockHistory where Code='{code.Code}' and date(hisDate) >= date('{hisDateStart}') and date(hisDate) <= date('{hisDateEnd}') order by hisDate desc limit {risingDays}", conn);
                    dt = new DataTable();
                    ada.Fill(dt);
                    var cnt = 0;
                    if (dt.Rows.Count < risingDays)
                    {
                        continue;
                    }
                    foreach (DataRow dr in dt.Rows)
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
                    if (cnt > 0 && cnt == dt.Rows.Count) // 所有天都是阳线收盘
                    {
                        ++pos;
                        //Console.WriteLine(code.Code);
                        detailBuilder.Append($"<tr><td>{pos}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                    }
                }
                detailBuilder.Append($"</table>");
                swReport.WriteLine(detailBuilder);
                swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
                swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
                swReport.WriteLine($"<h5>软件版本：{1.1}</h5>");
                swReport.WriteLine($"</div>");
                swReport.WriteLine($"</body>");
                swReport.WriteLine($"<html>");
                swReport.Close();
                File.Copy(Path.Combine(fi.Directory.FullName, $"RiskNDaysStock.html"), Path.Combine(hisPath, $"RiskNDaysStock-{reportDateTime}.html"), true);

            }
            Console.WriteLine("OK!");
        }

        public static string GetStockName(string code)
        {
            var s = GetHtmltxt($"http://hq.sinajs.cn/list={code}");
            var arr = s.Split('=');
            if (arr[1] == "\"\";\n" || arr[1] == "FAILED")
            {
                // 不存在
                return string.Empty;
            }
            else if (s.Length > 30)
            {
                // 存在
                var data = arr[1].Substring(1).Split(',');
                return data[0]; // 名称
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetHtmltxt(string url)
        {
            string str;

            WebRequest web = WebRequest.Create(url);
            web.Method = "GET";
            HttpWebResponse httpWeb = (HttpWebResponse)web.GetResponse();
            Stream stream = httpWeb.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            str = reader.ReadToEnd();
            stream.Close();
            reader.Close();

            return str;
        }

        /// <summary>
        /// 分时图的Base64
        /// </summary>
        /// <param name="stockCode"></param>
        /// <returns></returns>
        public static string GetFenShiImageBase64(string stockCode)
        {
            var webreq = System.Net.WebRequest.Create($"http://image.sinajs.cn/newchart/min/n/{stockCode}.gif?_=" + DateTime.Now.Ticks);
            using (var webres = webreq.GetResponse())
            {
                using (var stream = webres.GetResponseStream())
                {
                    using (var image = Image.FromStream(stream))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Gif);
                            byte[] arr = new byte[ms.Length];
                            ms.Position = 0;
                            ms.Read(arr, 0, (int)ms.Length);
                            return Convert.ToBase64String(arr);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 日K线图的Base64
        /// </summary>
        /// <param name="stockCode"></param>
        /// <returns></returns>
        public static string GetRiKXianImageBase64(string stockCode)
        {
            var webreq = System.Net.WebRequest.Create($"http://image.sinajs.cn/newchart/daily/n/{stockCode}.gif?_=" + DateTime.Now.Ticks);
            using (var webres = webreq.GetResponse())
            {
                using (var stream = webres.GetResponseStream())
                {
                    using (var image = Image.FromStream(stream))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Gif);
                            byte[] arr = new byte[ms.Length];
                            ms.Position = 0;
                            ms.Read(arr, 0, (int)ms.Length);
                            return Convert.ToBase64String(arr);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 周K线图的Base64
        /// </summary>
        /// <param name="stockCode"></param>
        /// <returns></returns>
        public static string GetZhouKXianImageBase64(string stockCode)
        {
            var webreq = System.Net.WebRequest.Create($"http://image.sinajs.cn/newchart/weekly/n/{stockCode}.gif?_=" + DateTime.Now.Ticks);
            using (var webres = webreq.GetResponse())
            {
                using (var stream = webres.GetResponseStream())
                {
                    using (var image = Image.FromStream(stream))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Gif);
                            byte[] arr = new byte[ms.Length];
                            ms.Position = 0;
                            ms.Read(arr, 0, (int)ms.Length);
                            return Convert.ToBase64String(arr);
                        }
                    }
                }
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
        }
    }
}
