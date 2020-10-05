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

namespace RisingStocks
{
    class Program
    {
        static string connectionString = string.Empty;
        static List<RowData> lst = new List<RowData>();
        static void Main(string[] args)
        {
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json")));
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);

            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"select * from StockHistory", conn);
                var dt = new DataTable();
                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(new RowData
                    {
                        Code = dr["Code"].ToString(),
                        HisDate = (DateTime)dr["HisDate"],
                        JinKai = (decimal)dr["JinKai"],
                        ZuiGao = (decimal)dr["ZuiGao"],
                        ShangZhangJinE = (decimal)dr["ShangZhangJinE"],
                        ShangZhangFuDu = (decimal)dr["ShangZhangFuDu"],
                        ZuiDi = (decimal)dr["ZuiDi"],
                        ShouPan = (decimal)dr["ShouPan"],
                        ChengJiaoLiang = (decimal)dr["ChengJiaoLiang"],
                        ChengJiaoE = (decimal)dr["ChengJiaoE"],
                        HuanShouLv = (decimal)dr["HuanShouLv"],
                    });
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
}
