﻿using System;
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
            var dt = new DataTable();
            //var sw = new StreamWriter(Path.Combine(fi.Directory.FullName, $"RecommandStock.csv"), false, Encoding.UTF8);
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
                        name.EndsWith("退") ||
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
                        //else
                        //{
                        //    var f = (int)(Math.Round(Convert.ToSingle(a / zhangFuDianShu), 3) * 100);
                        //    if (f < 20) // 可以参数化（20）
                        //    {
                        //        // 超短头发阳柱
                        //        tLevel = 1;
                        //    }
                        //    else
                        //    {
                        //        // 忽略，不符合条件
                        //    }
                        //}
                        #endregion
                        if (hLevel > 0 && tLevel > 0)
                        {
                            // 满足条件
                            var level = hLevel + tLevel;
                            if (level > 3)
                            {
                                lstRecommandStock.Add(new RecommendStock { Code = code, Exchange = exchange, Name = name, Level = level.ToString(), Price = price.ToString(), JinRiZuiDiDieFu = _jinRiZuiDiDieFu.ToString(), XiaYingXianChangDu = xiaYingXianChangDu.ToString() }); ;
                            }
                        }
                    }
                }
                //sw.WriteLine($"代码,名称,当前价,推荐等级");
                swReport.WriteLine($"<table class='table table-striped table-bordered table-hover'>");
                swReport.WriteLine($"<tr><th>序号</th><th>代码</th><th>名称</th><th>当前价</th><th>推荐等级</th><th>下影线长度(%)</th><th>今日最低跌幅(%)</th></tr>");
                lstRecommandStock.Sort(new CompareStock<RecommendStock>());
                int pos = 0;
                foreach (var item in lstRecommandStock)
                {
                    //sw.WriteLine($"{item.Exchange}{item.Code},{item.Name},{item.Price},{item.Level}");
                    swReport.WriteLine($"<tr><td>{++pos}</td><td>{item.Exchange}{item.Code}</td><td><a target='_blank' href='http://quote.eastmoney.com/{item.Code}.html'>{item.Name}</a></td><td>{item.Price}</td><td>{item.Level}</td><td>{item.XiaYingXianChangDu}</td><td>{item.JinRiZuiDiDieFu}</td></tr>");
                }
                //sw.Close();
                swReport.WriteLine($"</table>");
                swReport.WriteLine($"<div style='margin:0;padding:0;text-align:center'>");
                swReport.WriteLine($"<h5>生成时间：{reportDateTime}</h5>");
                swReport.WriteLine($"<h5>软件版本：{1.2}</h5>");
                swReport.WriteLine($"</div>");
                swReport.WriteLine($"</body>");
                swReport.WriteLine($"<html>");
                swReport.Close();
                //File.Copy(Path.Combine(fi.Directory.FullName, $"RecommandStock.csv"), Path.Combine(hisPath, $"RecommandStock-{reportDateTime}.csv"), true);
                File.Copy(Path.Combine(fi.Directory.FullName, $"RecommandStock.html"), Path.Combine(hisPath, $"RecommandStock-{reportDateTime}.html"), true);

            }
            Console.WriteLine($"OK!");
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
