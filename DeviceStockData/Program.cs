using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeviceStockData
{
    class Program
    {
        static List<string> codeList = new List<string>();
        static string connectionString = string.Empty;
        static string codesTableName = string.Empty;
        static List<RowData> rowDatas = new List<RowData>();
        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            codesTableName = jo["CodeSource"].ToString();

            var startDate = jo["StartDate"].ToString();
            var endDate = jo["EndDate"].ToString();

            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"{codesTableName}", conn);
                var dt = new DataTable();
                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add($"{dr["Code"].ToString()}");
                }
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmdDelete = new SQLiteCommand($"delete from StockHistory", conn);
                cmdDelete.ExecuteNonQuery();
                conn.Close();
            }
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var trans = conn.BeginTransaction();

                foreach (var stockCode in codeList)
                {
                    var q = $"https://q.stock.sohu.com/hisHq?code=cn_{stockCode}&start={startDate}&end={endDate}&stat=1&order=D&period=d&callback=historySearchHandler&rt=jsonp";
                    var ___s = GetHtmltxt(q);
                    var jsonData = ___s.Replace("historySearchHandler(", string.Empty).Replace("\n", string.Empty).TrimEnd(')');
                    Console.WriteLine(stockCode);
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
                        row.Code = stockCode;
                        row.HisDate = Convert.ToDateTime(data[0], new DateTimeFormatInfo { ShortDatePattern = "yyyy-MM-dd" });
                        decimal.TryParse(data[1].ToString(), out decimal jinKai); // 今开
                        decimal.TryParse(data[2].ToString(), out decimal zuiGao); // 最高
                        decimal.TryParse(data[3].ToString(), out decimal shangZhangJinE); // 上涨金额
                        decimal.TryParse(data[4].ToString().TrimEnd('%'), out decimal shangZhangFuDu); // 上涨幅度
                        decimal.TryParse(data[5].ToString(), out decimal zuiDi); // 最低
                        decimal.TryParse(data[6].ToString(), out decimal shouPan); // 收盘
                        decimal.TryParse(data[7].ToString(), out decimal chengJiaoLiang); // 成交量
                        decimal.TryParse(data[8].ToString(), out decimal chengJiaoE); // 成交额
                        decimal.TryParse(data[9].ToString(), out decimal huanShouLv); // 换手率
                        row.JinKai = jinKai;
                        row.ZuiGao = zuiGao;
                        row.ShangZhangJinE = shangZhangJinE;
                        row.ShangZhangFuDu = shangZhangFuDu;
                        row.ZuiDi = zuiDi;
                        row.ShouPan = shouPan;
                        row.ChengJiaoLiang = chengJiaoLiang;
                        row.ChengJiaoE = chengJiaoE;
                        row.HuanShouLv = huanShouLv;

                        var cmd = new SQLiteCommand($"INSERT INTO StockHistory(Code, HisDate, JinKai, ZuiGao, ShangZhangJinE, ShangZhangFuDu, ZuiDi, ShouPan, ChengJiaoLiang, ChengJiaoE, HuanShouLv) VALUES (@Code, @HisDate, @JinKai, @ZuiGao, @ShangZhangJinE, @ShangZhangFuDu, @ZuiDi, @ShouPan, @ChengJiaoLiang, @ChengJiaoE, @HuanShouLv)", conn, trans);
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SQLiteParameter("@Code", row.Code));
                        cmd.Parameters.Add(new SQLiteParameter("@HisDate", row.HisDate));
                        cmd.Parameters.Add(new SQLiteParameter("@JinKai", row.JinKai));
                        cmd.Parameters.Add(new SQLiteParameter("@ZuiGao", row.ZuiGao));
                        cmd.Parameters.Add(new SQLiteParameter("@ShangZhangJinE", row.ShangZhangJinE));
                        cmd.Parameters.Add(new SQLiteParameter("@ShangZhangFuDu", row.ShangZhangFuDu));
                        cmd.Parameters.Add(new SQLiteParameter("@ZuiDi", row.ZuiDi));
                        cmd.Parameters.Add(new SQLiteParameter("@ShouPan", row.ShouPan));
                        cmd.Parameters.Add(new SQLiteParameter("@ChengJiaoLiang", row.ChengJiaoLiang));
                        cmd.Parameters.Add(new SQLiteParameter("@ChengJiaoE", row.ChengJiaoE));
                        cmd.Parameters.Add(new SQLiteParameter("@HuanShouLv", row.HuanShouLv));
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
                trans.Commit();
            }
            Console.WriteLine("OK!");
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
    }

    public class RowData
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }
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
}
