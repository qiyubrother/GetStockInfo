using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetStockInfoFast
{
    class Program
    {
        static List<string> codeList = new List<string>();
        static string connectionString = string.Empty;
        static string codesTableName = string.Empty;
        static ConcurrentBag<RowData> rowDatas = new ConcurrentBag<RowData>();

        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            codesTableName = jo["CodeSource"].ToString();

            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"{codesTableName}", conn);
                var dt = new DataTable();
                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    codeList.Add($"{dr["Exchange"].ToString()}{dr["Code"].ToString()}");
                }
            }
            codeList.AsParallel().ForAll((code) =>
            {
                var s = GetHtmltxt($"http://hq.sinajs.cn/list={code}");
                var arr = s.Split('=');
                if (arr[1] == "\"\";\n" || arr[1] == "FAILED")
                {
                    // 不存在
                }
                else if (s.Length > 30)
                {
                    // 存在
                    Console.WriteLine($"{code}");
                    var data = arr[1].Substring(1).Split(',');
                    var name = data[0]; // 名称
                    decimal.TryParse(data[1], out decimal jinKai); // 今开
                    decimal.TryParse(data[2], out decimal zuoShou); // 昨收
                    decimal.TryParse(data[3], out decimal price); // 当前价格
                    decimal.TryParse(data[4], out decimal zuiGao); // 最高
                    decimal.TryParse(data[5], out decimal zuiDi); // 最低
                    var rowData = new RowData { 
                        Code = code.Substring(2), 
                        Exchange = code.Substring(0, 2),
                        Name = name,
                        Price = price,
                        ZuoShou = zuoShou,
                        JinKai = jinKai,
                        ZuiGao = zuiGao,
                        ZuiDi = zuiDi,
                        Timestamp = DateTime.Now
                    };
                    rowDatas.Add(rowData);
                }
            });
            Console.WriteLine("Save to database!");
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmdFast = new SQLiteCommand($"PRAGMA synchronous = OFF;", conn);
                cmdFast.ExecuteNonQuery();

                var pos = 1;
                foreach(var data in rowDatas)
                {
                    Console.WriteLine($"{pos++}/{rowDatas.Count}");
                    var ada = new SQLiteDataAdapter($"select count(*) from StockStatus where Code = '{data.Code}'", conn);
                    var dt = new DataTable();
                    ada.Fill(dt);
                    if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                    {
                        var cmdDelete = new SQLiteCommand($"delete from StockStatus where Code = '{data.Code}'", conn);
                        cmdDelete.ExecuteNonQuery();
                    }
                    if (data.Name.StartsWith("ST") ||
                        data.Name.StartsWith("*ST") ||
                        data.Name.StartsWith("N") ||
                        data.Price == 0
                        )
                    {
                        continue;
                    }
                    var cmd = new SQLiteCommand($"INSERT INTO StockStatus(Code, Exchange, Name, Price, ZuoShou, JinKai, ZuiGao, ZuiDi, Timestamp) VALUES (@Code, @Exchange, @Name, @Price, @ZuoShou, @JinKai, @ZuiGao, @ZuiDi, @Timestamp)", conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SQLiteParameter("@Code", data.Code));
                    cmd.Parameters.Add(new SQLiteParameter("@Exchange", data.Exchange.ToString()));
                    cmd.Parameters.Add(new SQLiteParameter("@Name", data.Name));
                    cmd.Parameters.Add(new SQLiteParameter("@Price", data.Price));
                    cmd.Parameters.Add(new SQLiteParameter("@ZuoShou", data.ZuoShou));
                    cmd.Parameters.Add(new SQLiteParameter("@JinKai", data.JinKai));
                    cmd.Parameters.Add(new SQLiteParameter("@ZuiGao", data.ZuiGao));
                    cmd.Parameters.Add(new SQLiteParameter("@ZuiDi", data.ZuiDi));
                    cmd.Parameters.Add(new SQLiteParameter("@Timestamp", data.Timestamp));
                    cmd.ExecuteNonQuery();
                }
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
        /// 股票名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 昨收
        /// </summary>
        public decimal ZuoShou { get; set; }
        /// <summary>
        /// 今开
        /// </summary>
        public decimal JinKai { get; set; }
        /// <summary>
        /// 最高
        /// </summary>
        public decimal ZuiGao { get; set; }
        /// <summary>
        /// 最低
        /// </summary>
        public decimal ZuiDi { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
