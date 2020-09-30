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
using GetStockInfo;
using System.Diagnostics;

namespace RecommendStock
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = string.Empty;
            var si = new StockInfo();
            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var s = File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json"));
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
            connectionString = jo["ConnectionString"].ToString();
            connectionString = connectionString.Replace("<path>", dbPath);
            var dt = new DataTable();
            var sw = new StreamWriter(System.IO.Path.Combine(fi.Directory.FullName, $"RecommandStock.csv"), false, Encoding.UTF8);
            var lstRecommandStock = new List<RecommendStock>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                var ada = new SQLiteDataAdapter($"select * from StockStatus", conn);

                ada.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    //si.Code = new StockCode { Code = dr["Code"].ToString(), Exchange = dr["Exchange"].ToString().ToLower() == "sh" ? Exchange.SH : Exchange.SZ };
                    //si.Name = dr["Name"].ToString(); // 名称
                    //si.Price = dr["Price"].ToString(); // 价格
                    //si.ZhangFuDianShu = dr["ZhangFuDianShu"].ToString(); // 涨幅
                    //si.ZhangFuBiLv = dr["ZhangFuBiLv"].ToString(); // 涨幅比率
                    //si.ZuoShou = dr["ZuoShou"].ToString(); // 昨收
                    //si.JinKai = dr["JinKai"].ToString(); // 今开
                    //si.ZuiGao = dr["ZuiGao"].ToString(); // 最高
                    //si.ZuiDi = dr["ZuiDi"].ToString(); // 最低
                    //si.ChengJiaoLiang = dr["ChengJiaoLiang"].ToString(); // 成交量
                    //si.ChengJiaoE = dr["ChengJiaoE"].ToString(); // 成交额
                    //si.ZongShiZhi = dr["ZongShiZhi"].ToString(); // 总市值
                    //si.LiuTongShiZhi = dr["LiuTongShiZhi"].ToString(); // 流通市值
                    //si.HuanShouLv = dr["HuanShouLv"].ToString(); // 换手率
                    //si.ShiJingLv = dr["ShiJingLv"].ToString(); // 市净率
                    //si.ZhenFu = dr["ZhenFu"].ToString(); // 振幅
                    //si.ShiYingLv = dr["ShiYingLv"].ToString(); // 市盈率

                    var code = dr["Code"].ToString();
                    var exchange = dr["Exchange"].ToString().ToLower();
                    var name = dr["Name"].ToString(); // 名称
                    decimal.TryParse(dr["Price"].ToString(), out decimal price); // 价格
                    decimal.TryParse(dr["ZhangFuDianShu"].ToString(), out decimal zhangFuDianShu); // 涨幅
                    decimal.TryParse(dr["ZhangFuBiLv"].ToString(), out decimal zhangFuBiLv); // 涨幅比率
                    decimal.TryParse(dr["ZuoShou"].ToString(), out decimal zuoShou); // 昨收
                    decimal.TryParse(dr["JinKai"].ToString(), out decimal jinKai); // 今开
                    decimal.TryParse(dr["ZuiGao"].ToString(), out decimal zuiGao) ; // 最高
                    decimal.TryParse(dr["ZuiDi"].ToString(), out decimal zuiDi); // 最低
                    decimal.TryParse(dr["ChengJiaoLiang"].ToString(), out decimal chengJiaoLiang); // 成交量
                    decimal.TryParse(dr["ChengJiaoE"].ToString(), out decimal chengJiaoE); // 成交额
                    decimal.TryParse(dr["ZongShiZhi"].ToString(), out decimal zongShiZhi); // 总市值
                    decimal.TryParse(dr["LiuTongShiZhi"].ToString(), out decimal liuTongShiZhi); // 流通市值
                    decimal.TryParse(dr["HuanShouLv"].ToString(), out decimal huanShouLv); // 换手率
                    decimal.TryParse(dr["ShiJingLv"].ToString(), out decimal shiJingLv); // 市净率
                    decimal.TryParse(dr["ZhenFu"].ToString(), out decimal zhenFu); // 振幅
                    decimal.TryParse(dr["ShiYingLv"].ToString(), out decimal shiYingLv); // 市盈率

                    if (name.StartsWith("ST") ||
                        name.StartsWith("*ST") ||
                        name.StartsWith("N") ||
                        price == 0 ||
                        zhangFuDianShu == 0
                        )
                    {
                        continue;
                    }
                    if (jinKai >= zuoShou && zuiDi > 0) // 高开
                    {
                        var hLevel = 0;
                        var tLevel = 0;
                        #region 阳腿
                        var b = jinKai - zuiDi; // 腿的长度
                        if (b == 0)
                        {
                            // 无腿阳柱
                            hLevel = 2;
                        }
                        else
                        {
                            var f = (int)(Math.Round(Convert.ToSingle(b / zhangFuDianShu), 3) * 100);
                            if (f < 10) // 可以参数化（10）
                            {
                                // 超短腿阳柱
                                hLevel = 1;
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
                            tLevel = 2;
                        }
                        else
                        {
                            var f = (int)(Math.Round(Convert.ToSingle(a / zhangFuDianShu), 3) * 100);
                            if (f < 20) // 可以参数化（20）
                            {
                                // 超短头发阳柱
                                tLevel = 1;
                            }
                            else
                            {
                                // 忽略，不符合条件
                            }
                        }
                        #endregion
                        if (hLevel > 0 && tLevel > 0)
                        {
                            // 满足条件
                            var level = hLevel + tLevel;
                            lstRecommandStock.Add(new RecommendStock { Code = code, Exchange = exchange, Name = name, Level = level.ToString(), Price = price.ToString() });
                        }
                    }
                }
                sw.WriteLine($"代码,名称,当前价,推荐等级");
                foreach (var item in lstRecommandStock)
                {
                    sw.WriteLine($"{item.Exchange}{item.Code},{item.Name},{item.Price},{item.Level}");
                }
                sw.Close();
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
    }
}
