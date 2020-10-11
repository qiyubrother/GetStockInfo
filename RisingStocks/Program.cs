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
        static int fallingDays = 5;
        static int increaseDays = 5;
        static int decreaseDays = 5;
        static float increase = 8;
        static float decrease = 20;
        static List<RowData> lst = new List<RowData>();
        static List<Codes> codeList = new List<Codes>();
        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var hisPath = Path.Combine(fi.Directory.Parent.Parent.FullName, "history");
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            codesTableName = jo["CodeSource"].ToString();
            risingDays = Convert.ToInt32(jo["RisingDays"].ToString()); // 连续阳线天数
            fallingDays = Convert.ToInt32(jo["FallingDays"].ToString()); // 连续阴线天数
            increaseDays = Convert.ToInt32(jo["Increase"].ToString().Split(',')[0]); // 连续上涨天数
            increase = Convert.ToSingle(jo["Increase"].ToString().Split(',')[1]); // 连续涨幅
            decreaseDays = Convert.ToInt32(jo["Decrease"].ToString().Split(',')[0]); // 连续上涨天数
            decrease = Convert.ToSingle(jo["Decrease"].ToString().Split(',')[1]); // 连续跌幅

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
            var risingDetailBuilder = new StringBuilder();
            risingDetailBuilder.Append($"<table class='table table-striped table-bordered table-hover'>");
            risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sz}.html'>上证指数</a><br>{sz}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(sz)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(sz)}' /></td></tr>");
            risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{sc}.html'>深证成指</a><br>{sc}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(sc)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(sc)}' /></td></tr>");
            risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{cy}.html'>创业板指</a><br>{cy}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(cy)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(cy)}' /></td></tr>");
            risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{zx}.html'>中小板指</a><br>{zx}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(zx)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(zx)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(zx)}' /></td></tr>");
            risingDetailBuilder.Append($"<tr><td>&nbsp;</td><td><a target='_blank' href='http://quote.eastmoney.com/{kc50}.html'>科创50指数</a><br>{kc50}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(kc50)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(kc50)}' /></td></tr>");
            var fallingDetailBuilder = new StringBuilder();
            var increaseDetailBuilder = new StringBuilder();
            var decreaseDetailBuilder = new StringBuilder();
            int posYang = 0;
            int posYin = 0;
            int posIncrease = 0;
            int posDecrease = 0;
            //Console.WriteLine($"connectionString:{connectionString}");
            //Console.WriteLine($"codesTableName:{codesTableName}");
            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"{codesTableName}", conn);
                var dt = new DataTable();
                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add(new Codes { Code = dr["Code"].ToString(), Exchange = dr["Exchange"].ToString() });
                }
                // Console.WriteLine($"dt:{dt.Rows.Count}");
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

                    var adaR = new SQLiteDataAdapter($"select * from StockHistory where Code='{code.Code}' and date(hisDate) >= date('{hisDateStart}') and date(hisDate) <= date('{hisDateEnd}') order by hisDate desc limit {risingDays}", conn);
                    var dtR = new DataTable();
                    adaR.Fill(dtR);

                    var adaF = new SQLiteDataAdapter($"select * from StockHistory where Code='{code.Code}' and date(hisDate) >= date('{hisDateStart}') and date(hisDate) <= date('{hisDateEnd}') order by hisDate desc limit {fallingDays}", conn);
                    var dtF = new DataTable();
                    adaF.Fill(dtF);

                    var adaI = new SQLiteDataAdapter($"select * from StockHistory where Code='{code.Code}' and date(hisDate) >= date('{hisDateStart}') and date(hisDate) <= date('{hisDateEnd}') order by hisDate desc limit {increaseDays}", conn);
                    var dtI = new DataTable();
                    adaI.Fill(dtI);

                    var adaD = new SQLiteDataAdapter($"select * from StockHistory where Code='{code.Code}' and date(hisDate) >= date('{hisDateStart}') and date(hisDate) <= date('{hisDateEnd}') order by hisDate desc limit {decreaseDays}", conn);
                    var dtD = new DataTable();
                    adaD.Fill(dtD);

                    var _rank = GetIndustryRank(code.Code, conn);
                    var rank = _rank == string.Empty ? string.Empty : $"<br />R{_rank}";

                    #region 计算连续阳线天数
                    if (dtR.Rows.Count >= risingDays)
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
                            var __ada = new SQLiteDataAdapter($"select * from StockIndustry where Code='{code.Code}'", conn);
                            var __dt = new DataTable();
                            __ada.Fill(__dt);
                            if (__dt.Rows.Count > 0)
                            {
                                industry = __dt.Rows[0]["Industry"].ToString();
                                industryHref = __dt.Rows[0]["Href"].ToString();
                            }
                            risingDetailBuilder.Append($"<tr><td style='color:red;'>R{risingDays}-{posYang}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                        }
                    }
                    #endregion
                    #region 计算连续阴线天数
                    if (dtF.Rows.Count >= fallingDays)
                    {
                        var cnt = 0;
                        foreach (DataRow dr in dtF.Rows)
                        {
                            if ((decimal)dr["ShouPan"] < (decimal)dr["JinKai"])
                            {
                                cnt++;
                            }
                            else
                            {
                                cnt = 0;
                                break;
                            }
                        }
                        if (cnt > 0 && cnt == dtF.Rows.Count) // 所有天都是阴线收盘
                        {
                            ++posYin;
                            var industry = string.Empty;
                            var industryHref = "#";
                            var __ada = new SQLiteDataAdapter($"select * from StockIndustry where Code='{code.Code}'", conn);
                            var __dt = new DataTable();
                            __ada.Fill(__dt);
                            if (__dt.Rows.Count > 0)
                            {
                                industry = __dt.Rows[0]["Industry"].ToString();
                                industryHref = __dt.Rows[0]["Href"].ToString();
                            }
                            fallingDetailBuilder.Append($"<tr><td style='color:darkgreen;'>F{fallingDays}-{posYin}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                        }
                    }
                    #endregion
                    #region 计算连续N天上涨百分之P的股票
                    if (dtI.Rows.Count >= increaseDays)
                    {
                        var p1 = Convert.ToSingle(dtI.Rows[increaseDays - 1]["ShouPan"].ToString());
                        var p2 = Convert.ToSingle(dtI.Rows[0]["ShouPan"].ToString());
                        var m = (float)Math.Round((p2 - p1) / p1 * 100, 2);
                        //Console.WriteLine($"计算连续N天上涨百分之P的股票...");
                        if (m > increase)
                        {
                            ++posIncrease;
                            var industry = string.Empty;
                            var industryHref = "#";
                            var __ada = new SQLiteDataAdapter($"select * from StockIndustry where Code='{code.Code}'", conn);
                            var __dt = new DataTable();
                            __ada.Fill(__dt);
                            if (__dt.Rows.Count > 0)
                            {
                                industry = __dt.Rows[0]["Industry"].ToString();
                                industryHref = __dt.Rows[0]["Href"].ToString();
                            }
                            increaseDetailBuilder.Append($"<tr><td style='color:red;'>I{increaseDays},{increase}%-{posIncrease}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                        }
                    }
                    #endregion
                    #region 连续N天下降百分之P的股票
                    if (dtD.Rows.Count >= decreaseDays)
                    {
                        var p1 = Convert.ToSingle(dtD.Rows[decreaseDays - 1]["ShouPan"].ToString());
                        var p2 = Convert.ToSingle(dtD.Rows[0]["ShouPan"].ToString());
                        var m = (float)Math.Round((p1 - p2) / p1 * 100, 2);
                        //Console.WriteLine($"连续N天下降百分之P的股票...");
                        if (m > decrease)
                        {
                            ++posDecrease;
                            var industry = string.Empty;
                            var industryHref = "#";
                            var __ada = new SQLiteDataAdapter($"select * from StockIndustry where Code='{code.Code}'", conn);
                            var __dt = new DataTable();
                            __ada.Fill(__dt);
                            if (__dt.Rows.Count > 0)
                            {
                                industry = __dt.Rows[0]["Industry"].ToString();
                                industryHref = __dt.Rows[0]["Href"].ToString();
                            }
                            decreaseDetailBuilder.Append($"<tr><td style='color:darkgreen;'>D{decreaseDays},{decrease}%-{posDecrease}</td><td><a target='_blank' href='http://quote.eastmoney.com/{code.Code}.html'>{name}</a><br />{code.Exchange}{code.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(code.Exchange + code.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(code.Exchange + code.Code)}' /></td></tr>");
                        }
                    }
                    #endregion
                }

                risingDetailBuilder.Append(fallingDetailBuilder);
                risingDetailBuilder.Append(increaseDetailBuilder);
                risingDetailBuilder.Append(decreaseDetailBuilder);

                risingDetailBuilder.Append($"</table>");
                swReport.WriteLine(risingDetailBuilder);
                swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
                swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
                swReport.WriteLine($"<h5>软件版本：{1.2}</h5>");
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

        public static string GetIndustryRank(string stockCode, SQLiteConnection conn)
        {
            var __ada = new SQLiteDataAdapter($"select * from IndustryRanking where Code='{stockCode}'", conn);
            var __dt = new DataTable();
            __ada.Fill(__dt);
            if (__dt.Rows.Count > 0)
            {
                return __dt.Rows[0]["Rank"].ToString();
            }
            return string.Empty;
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
