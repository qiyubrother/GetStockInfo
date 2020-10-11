using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace RecommendStock
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = string.Empty;
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var s = File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json"));
            var dbPath = Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            var hisPath = Path.Combine(fi.Directory.Parent.Parent.FullName, "history");
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            var paramXiaYingXianChangDu = Convert.ToInt32(jo["XiaYingXianChangDu"].ToString()); // 下影线长度
            var paramJinRiZuiDiDieFu = Convert.ToSingle(jo["JinRiZuiDiDieFu"].ToString()); // 今日最低跌幅
            var paramJianYiMaiChuXiShu = Convert.ToSingle(jo["JianYiMaiChuXiShu"].ToString()); // 建议卖出系数
            var paramZuiDiZhangFu = Convert.ToSingle(jo["ZuiDiZhangFu"].ToString()); // 最低涨幅(%)
            var paramZuiGaoZhangFu = Convert.ToSingle(jo["ZuiGaoZhangFu"].ToString()); // 最高涨幅(%)
            var paramMai1DaYuLing = Convert.ToBoolean(jo["Mai1DaYuLing"].ToString()); // 卖1大于0

            var dt = new DataTable();
            var swReport = new StreamWriter(Path.Combine(fi.Directory.FullName, $"RecommandStock.html"), false, Encoding.UTF8);
            var lstRecommandStock = new List<RecommendStock>();
            var reportDateTime = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}-{DateTime.Now.Hour.ToString().PadLeft(2, '0')}{DateTime.Now.Minute.ToString().PadLeft(2, '0')}";
            swReport.WriteLine($"<!DOCTYPE html>");
            swReport.WriteLine($"<html>");
            swReport.WriteLine($"<head>");
            swReport.WriteLine($"<meta charset=\"UTF-8\">");
            swReport.WriteLine($"<link rel='stylesheet' href='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/css/bootstrap.min.css'>  ");
            swReport.WriteLine($"<script src='https://cdn.staticfile.org/jquery/2.1.1/jquery.min.js'></script>");
            swReport.WriteLine($"<script src='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/js/bootstrap.min.js'></script>");
            swReport.WriteLine($"<title>Recommend Stock Report</title>");
            swReport.WriteLine($"</head>");
            swReport.WriteLine($"<body>");
            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"select * from StockStatus", conn);

                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    var code = dr["Code"].ToString();
                    var exchange = dr["Exchange"].ToString().ToLower();
                    var name = dr["Name"].ToString(); // 名称
                    decimal.TryParse(dr["Price"].ToString(), out decimal price); // 价格
                    decimal.TryParse(dr["ZuoShou"].ToString(), out decimal zuoShou); // 昨收
                    decimal.TryParse(dr["JinKai"].ToString(), out decimal jinKai); // 今开
                    decimal.TryParse(dr["ZuiGao"].ToString(), out decimal zuiGao) ; // 最高
                    decimal.TryParse(dr["ZuiDi"].ToString(), out decimal zuiDi); // 最低

                    var zhangFuDianShu = price - jinKai;

                    if (name.StartsWith("ST") ||
                        name.StartsWith("*ST") ||
                        name.StartsWith("N") ||
                        name.Contains("退") ||
                        price == 0 ||
                        zhangFuDianShu == 0 ||
                        jinKai == 0
                        )
                    {
                        continue;
                    }
                    var _jinRiZuiDiDieFu = Math.Round(Convert.ToSingle((jinKai - zuiDi) / jinKai * 100), 2);
                    int xiaYingXianChangDu = 0;
                    if (jinKai >= zuoShou && zuiDi > 0 && jinKai < price) // 高开
                    {
                        var hLevel = 0;
                        var tLevel = 0;
                        #region 阳腿
                        var b = jinKai - zuiDi; // 腿的长度
                        if (b == 0)
                        {
                            // 无腿阳柱
                            hLevel = 3;
                        }
                        else
                        {
                            xiaYingXianChangDu = (int)(Math.Round(Convert.ToSingle(b / zhangFuDianShu), 3) * 100);
                            if (xiaYingXianChangDu < 10) // 可以参数化
                            {
                                if (_jinRiZuiDiDieFu < paramJinRiZuiDiDieFu) // recomment: 2, 2.5, 3
                                {
                                    // 超短腿阳柱
                                    hLevel = 2;
                                }
                            }
                            if (xiaYingXianChangDu <= paramXiaYingXianChangDu /* recomment: 20, 21, 22, 23, 24, 25 */) // 可以参数化
                            {
                                if (_jinRiZuiDiDieFu < paramJinRiZuiDiDieFu) // recomment: 2, 2.5, 3
                                {
                                    // 短腿阳柱
                                    hLevel = 1;
                                }
                            }
                            else
                            {
                                // 忽略，不符合条件
                            }
                        }
                        #endregion
                        #region 阳头
                        var a = zuiGao - price; // 头发的长度
                        if (a == 0)
                        {
                            // 秃头阳柱
                            tLevel = 3;
                        }
                        #endregion
                        if (hLevel > 0 && tLevel > 0)
                        {
                            // 满足条件
                            var level = hLevel + tLevel;
                            if (level > 3)
                            {
                                if (paramMai1DaYuLing)
                                {
                                    var mai1Cnt = GetMai1(exchange + code).Split(',')[0].Trim();
                                    if (mai1Cnt != "0")
                                    {
                                        float v = (float)((price - jinKai) / jinKai * 100);
                                        if (v >= paramZuiDiZhangFu && v <= paramZuiGaoZhangFu)
                                        {
                                            lstRecommandStock.Add(new RecommendStock { Code = code, Exchange = exchange, Name = name, Level = level.ToString(), Price = price.ToString(), JinRiZuiDiDieFu = _jinRiZuiDiDieFu.ToString(), XiaYingXianChangDu = xiaYingXianChangDu.ToString() });
                                        }
                                    }
                                }
                                else
                                {
                                    float v = (float)((price - jinKai) / jinKai * 100);
                                    if (v >= paramZuiDiZhangFu && v <= paramZuiGaoZhangFu)
                                    {
                                        lstRecommandStock.Add(new RecommendStock { Code = code, Exchange = exchange, Name = name, Level = level.ToString(), Price = price.ToString(), JinRiZuiDiDieFu = _jinRiZuiDiDieFu.ToString(), XiaYingXianChangDu = xiaYingXianChangDu.ToString() });
                                    }
                                }
                            }
                        }
                    }
                }
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
                swReport.WriteLine($"<table class='table table-striped table-bordered table-hover'>");
                swReport.WriteLine($"<tr><th>序号</th><th>代码</th><th>名称</th><th>行业</th><th>当前价</th><th>推荐等级(数值越大越好)</th><th>下影线长度(%)(数值越小越好)</th><th>今日最低跌幅(%)(数值越小越好)</th><th>卖1(量,价)</th><th>建议卖出价(当前价的1.025倍)</th></tr>");
                lstRecommandStock.Sort(new CompareStock<RecommendStock>());
                int pos = 0;
                foreach (var item in lstRecommandStock)
                {
                    ++pos;
                    var industry = string.Empty;
                    var industryHref = "#";
                    var __ada = new SQLiteDataAdapter($"select * from StockIndustry where Code='{item.Code}'", conn);
                    var __dt = new DataTable();
                    __ada.Fill(__dt);
                    if (__dt.Rows.Count > 0)
                    {
                        industry = __dt.Rows[0]["Industry"].ToString();
                        industryHref = __dt.Rows[0]["Href"].ToString();
                    }
                    var _rank = GetIndustryRank(item.Code, conn);
                    var rank = _rank == string.Empty ? string.Empty : $"<br />R{_rank}";
                    swReport.WriteLine($"<tr><td>{pos}</td><td>{item.Exchange}{item.Code}</td><td><a target='_blank' href='http://quote.eastmoney.com/{item.Code}.html'>{item.Name}</a></td><td><a target='_blank' href='{industryHref}'>{industry}</a></td><td>{item.Price}</td><td>{item.Level}</td><td>{item.XiaYingXianChangDu}</td><td>{item.JinRiZuiDiDieFu}</td><td>{GetMai1(item.Exchange + item.Code)}</td><td>{Math.Round(Convert.ToSingle(item.Price) * paramJianYiMaiChuXiShu, 2)}</td></tr>");
                    detailBuilder.Append($"<tr><td>{pos}</td><td><a target='_blank' href='http://quote.eastmoney.com/{item.Code}.html'>{item.Name}</a><br>{item.Exchange}{item.Code}<br /><a target='_blank' href='{industryHref}'>{industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(item.Exchange+item.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(item.Exchange + item.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(item.Exchange + item.Code)}' /></td></tr>");
                }
                swReport.WriteLine($"</table>");
                detailBuilder.Append($"</table>");
                swReport.WriteLine(detailBuilder);
                swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
                swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
                swReport.WriteLine($"<h5>软件版本：{1.7}</h5>");
                swReport.WriteLine($"</div>");
                swReport.WriteLine($"</body>");
                swReport.WriteLine($"<html>");
                swReport.Close();
                File.Copy(Path.Combine(fi.Directory.FullName, $"RecommandStock.html"), Path.Combine(hisPath, $"RecommandStock-{reportDateTime}.html"), true);

            }
            Console.WriteLine($"OK!");
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
        /// <summary>
        /// 取得卖1
        /// </summary>
        /// <param name="stockCode"></param>
        /// <returns></returns>
        public static string GetMai1(string stockCode)
        {
            var s = GetHtmltxt($"http://hq.sinajs.cn/list={stockCode}");
            var arr = s.Split('=');
            if (arr[1] == "\"\";\n" || arr[1] == "FAILED")
            {
                // 不存在
                return string.Empty;
            }
            else if (s.Length > 30)
            {
                var data = arr[1].Substring(1).Split(',');
                try
                {
                    return $"{data[22]},{data[23]}";
                }
                catch
                {
                    return string.Empty;
                }
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
    }

    public class CompareStock<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            var a = x as RecommendStock;
            var b = y as RecommendStock;
            var ia = Convert.ToInt32(a.Level);
            var ib = Convert.ToInt32(b.Level);
            if (ia > ib)
            {
                return -1;
            }
            else if (ia == ib)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

    public class RecommendStock
    {
        public string Code { get; set; }
        public string Exchange { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string Price { get; set; }
        public string JinRiZuiDiDieFu { get; set; }
        public string XiaYingXianChangDu { get; set; }
    }
}
