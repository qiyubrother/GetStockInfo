using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Database;
namespace StockKit
{
    public class StockHelper
    {
        public static string MakeReport(string schemaId, string outFileName)
        {
            var dt = new DataTable();
            var swReport = new StreamWriter(outFileName, false, Encoding.UTF8);
            var reportDateTime = $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}-{DateTime.Now.Hour.ToString().PadLeft(2, '0')}{DateTime.Now.Minute.ToString().PadLeft(2, '0')}";
            var lstStockPool = new List<StockPool>();
            swReport.WriteLine($"<!DOCTYPE html>");
            swReport.WriteLine($"<html>");
            swReport.WriteLine($"<head>");
            swReport.WriteLine($"<meta charset=\"UTF-8\">");
            swReport.WriteLine($"<link rel='stylesheet' href='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/css/bootstrap.min.css'>  ");
            swReport.WriteLine($"<script src='https://cdn.staticfile.org/jquery/2.1.1/jquery.min.js'></script>");
            swReport.WriteLine($"<script src='https://cdn.staticfile.org/twitter-bootstrap/3.3.7/js/bootstrap.min.js'></script>");
            swReport.WriteLine($"<title>Quality Stock Pool</title>");
            swReport.WriteLine($"</head>");
            swReport.WriteLine($"<body>");
            dt = SqlServer.Instance.GetDataTable($"SELECT a.code, a.exchange, b.name, c.Industry, c.Href FROM [SchemaDetail] as a left join stockstatus as b on a.code = b.code left join StockIndustry as c on a.code = c.code where schemaId = '{schemaId}'");

            foreach (DataRow dr in dt.Rows)
            {
                var code = dr["Code"].ToString();
                var exchange = dr["Exchange"].ToString().ToLower();
                var name = dr["name"].ToString();
                var industry = dr["industry"].ToString();
                var href = dr["Href"].ToString();
                lstStockPool.Add(new StockPool { Code = code, Exchange = exchange, Name = name, Industry = industry, Href = href });
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
            int pos = 0;
            foreach (var item in lstStockPool)
            {
                ++pos;

                var rank = string.Empty;
                detailBuilder.Append($"<tr><td>{pos}</td><td><a target='_blank' href='http://quote.eastmoney.com/{item.Code}.html'>{item.Name}</a><br>{item.Exchange}{item.Code}<br /><a target='_blank' href='{item.Href}'>{item.Industry}</a>{rank}</td><td><image src='data:image/png;base64,{GetFenShiImageBase64(item.Exchange + item.Code)}' /></td><td><image src='data:image/png;base64,{GetRiKXianImageBase64(item.Exchange + item.Code)}' /></td><td><image src='data:image/png;base64,{GetZhouKXianImageBase64(item.Exchange + item.Code)}' /></td></tr>");
            }
            detailBuilder.Append($"</table>");
            swReport.WriteLine(detailBuilder);
            swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
            swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
            swReport.WriteLine($"<h5>软件版本：{2.1}</h5>");
            swReport.WriteLine($"</div>");
            swReport.WriteLine($"</body>");
            swReport.WriteLine($"<html>");
            swReport.Close();

            return outFileName;
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
        public static string GetStockName(string code)
        {
            var name = "(股票名称)";
            var s = GetHtmltxt($"http://hq.sinajs.cn/list={code}");
            var arr = s.Split('=');
            if (arr[1] == "\"\";\n" || arr[1] == "FAILED")
            {
                // 不存在
                return name;
            }
            else if (s.Length > 30)
            {
                // 存在
                var data = arr[1].Substring(1).Split(',');
                return data[0]; // 名称
            }
            return name;
        }
    }

    public class StockPool
    {
        public string Code { get; set; }
        public string Exchange { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Href { get; set; }
    }
}
