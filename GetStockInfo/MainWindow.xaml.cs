using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data.SQLite;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace GetStockInfo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        DispatcherTimer timer = new DispatcherTimer();
        public bool IsOK = false;
        int pos = 0;
        private List<string> codeList = new List<string>();
        string connectionString = string.Empty;
        string codesTableName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();

            var fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            var s = File.ReadAllText(System.IO.Path.Combine(fi.Directory.FullName, "config.json"));
            var dbPath = System.IO.Path.Combine(fi.Directory.Parent.Parent.FullName, "db");
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
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
            SuppressScriptErrors(web, true);

            web.Navigated += (o, ex) => {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Compute();
                    }
                    catch(Exception e)
                    {

                    }
                    pos++;
                    if (pos < codeList.Count)
                    {
                        GetStockInfo(codeList);
                    }
                    else
                    {
                        Close();
                    }
                }));
            };
            Loaded += (sender, e)=> GetStockInfo(codeList);
        }

        public void WriteData(StockInfo data)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand($"delete from StockStatus where Code = '{data.Code.Code}'", conn);
                cmd.ExecuteNonQuery();
                cmd = new SQLiteCommand($"INSERT INTO StockStatus(Code, Exchange, Name, Price, ZhangFuDianShu, ZhangFuBiLv, ZuoShou, JinKai, ZuiGao, ZuiDi, ChengJiaoLiang, ChengJiaoE, ZongShiZhi, LiuTongShiZhi, HuanShouLv, ShiJingLv, ZhenFu, ShiYingLv, Timestamp) VALUES (@Code, @Exchange, @Name, @Price, @ZhangFuDianShu, @ZhangFuBiLv, @ZuoShou, @JinKai, @ZuiGao, @ZuiDi, @ChengJiaoLiang, @ChengJiaoE, @ZongShiZhi, @LiuTongShiZhi, @HuanShouLv, @ShiJingLv, @ZhenFu, @ShiYingLv, @Timestamp)", conn);
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SQLiteParameter("@Code", data.Code.Code));
                cmd.Parameters.Add(new SQLiteParameter("@Exchange", data.Code.Exchange.ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@Name", data.Name));
                cmd.Parameters.Add(new SQLiteParameter("@Price", data.Price));
                cmd.Parameters.Add(new SQLiteParameter("@ZhangFuDianShu", data.ZhangFuDianShu));
                cmd.Parameters.Add(new SQLiteParameter("@ZhangFuBiLv", data.ZhangFuBiLv));
                cmd.Parameters.Add(new SQLiteParameter("@ZuoShou", data.ZuoShou));
                cmd.Parameters.Add(new SQLiteParameter("@JinKai", data.JinKai));
                cmd.Parameters.Add(new SQLiteParameter("@ZuiGao", data.ZuiGao));
                cmd.Parameters.Add(new SQLiteParameter("@ZuiDi", data.ZuiDi));
                cmd.Parameters.Add(new SQLiteParameter("@ChengJiaoLiang", data.ChengJiaoLiang));
                cmd.Parameters.Add(new SQLiteParameter("@ChengJiaoE", data.ChengJiaoE));
                cmd.Parameters.Add(new SQLiteParameter("@ZongShiZhi", data.ZongShiZhi));
                cmd.Parameters.Add(new SQLiteParameter("@LiuTongShiZhi", data.LiuTongShiZhi));
                cmd.Parameters.Add(new SQLiteParameter("@HuanShouLv", data.HuanShouLv));
                cmd.Parameters.Add(new SQLiteParameter("@ShiJingLv", data.ShiJingLv));
                cmd.Parameters.Add(new SQLiteParameter("@ZhenFu", data.ZhenFu));
                cmd.Parameters.Add(new SQLiteParameter("@ShiYingLv", data.ShiYingLv));
                cmd.Parameters.Add(new SQLiteParameter("@ZhenFu", data.ZhenFu));
                cmd.Parameters.Add(new SQLiteParameter("@ShiYingLv", data.ShiYingLv == "亏损"? "0" : data.ShiYingLv));
                cmd.Parameters.Add(new SQLiteParameter("@Timestamp", data.Timestamp));
                cmd.ExecuteNonQuery();
            }
        }

        public void GetStockInfo(List<string> codeList)
        {
            web.Navigate($"http://gu.qq.com/{codeList[pos]}/gp?_={DateTime.Now.Ticks}");
        }

        /// <summary>
        /// 在加载页面之前调用此方法设置hide为true就能抑制错误的弹出了。
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="hide"></param>
        static void SuppressScriptErrors(WebBrowser webBrowser, bool hide)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }
        private void Compute()
        {
            var doc = web.Document as mshtml.HTMLDocument;
            if (doc == null)
            {
                Console.WriteLine("doc is null.");
                return;
            }
            IHTMLElement mainPanel = doc.getElementById("hqpanel");

            StockInfo si = new StockInfo();

            if (mainPanel == null)
            {
                // 读取错误。
                Console.WriteLine("读取错误。");
                return;
            }

            foreach (IHTMLElement child in mainPanel.children)
            {
                if (child.tagName.ToUpper() == "DIV")
                {
                    if (child.className.Contains("gb_title"))
                    {
                        si.Name = ((child.children[0] as IHTMLElement).children[0] as IHTMLElement).innerText;
                        si.Code = StockCode.Parse(((child.children[0] as IHTMLElement).children[1] as IHTMLElement).innerText);

                        lblMessage.Content = $"{pos}/{codeList.Count} {si.Name} {si.Code}";
                    }
                    else if (child.className.Contains("content"))
                    {
                        if ((child.children[0] as IHTMLElement).className.Contains("col-1 fl"))
                        {
                            var currentPrice = (((child.children[0] as IHTMLElement).children[1] as IHTMLElement).children[0] as IHTMLElement).innerText; // 价格
                            var upNumber = ((((child.children[0] as IHTMLElement).children[1] as IHTMLElement).children[1] as IHTMLElement).children[0] as IHTMLElement).innerText; // 涨幅点数
                            var upPercentage = ((((child.children[0] as IHTMLElement).children[1] as IHTMLElement).children[1] as IHTMLElement).children[1] as IHTMLElement).innerText; // 涨幅比率
                            si.Price = currentPrice;
                            si.ZhangFuDianShu = upNumber;
                            si.ZhangFuBiLv = upPercentage.Replace("%", string.Empty);
                        }
                        if ((child.children[1] as IHTMLElement).className.Contains("col-2 fr"))
                        {
                            si.ZuoShou = (((((child.children[1] as IHTMLElement).children[0] as IHTMLElement)).children[0] as IHTMLElement).children[1] as IHTMLElement).innerText; // 昨收
                            si.JinKai = (((((child.children[1] as IHTMLElement).children[0] as IHTMLElement)).children[1] as IHTMLElement).children[1] as IHTMLElement).innerText; // 今开
                            si.ZuiGao = (((((child.children[1] as IHTMLElement).children[0] as IHTMLElement)).children[2] as IHTMLElement).children[1] as IHTMLElement).innerText; // 最高
                            si.ZuiDi = (((((child.children[1] as IHTMLElement).children[0] as IHTMLElement)).children[3] as IHTMLElement).children[1] as IHTMLElement).innerText; // 最低

                            si.ChengJiaoLiang = (((((child.children[1] as IHTMLElement).children[1] as IHTMLElement)).children[0] as IHTMLElement).children[1] as IHTMLElement).innerText.Replace("手", string.Empty); // 成交量
                            if (si.ChengJiaoLiang.Contains("万"))
                            {
                                si.ChengJiaoLiang = (((Convert.ToDecimal(si.ChengJiaoLiang.Replace("万", string.Empty))) * 10000)).ToString();
                            }
                            si.ChengJiaoE = (((((child.children[1] as IHTMLElement).children[1] as IHTMLElement)).children[1] as IHTMLElement).children[1] as IHTMLElement).innerText; // 成交额
                            if (si.ChengJiaoE.Contains("万"))
                            {
                                si.ChengJiaoE = (((Convert.ToDecimal(si.ChengJiaoLiang.Replace("万", string.Empty))) * 10000)).ToString();
                            }
                            if (si.ChengJiaoE.Contains("亿"))
                            {
                                si.ChengJiaoE = (((Convert.ToDecimal(si.ChengJiaoLiang.Replace("亿", string.Empty))) * 100000000)).ToString();
                            }
                            si.ZongShiZhi = (((((child.children[1] as IHTMLElement).children[1] as IHTMLElement)).children[2] as IHTMLElement).children[1] as IHTMLElement).innerText.Replace("亿", string.Empty); // 总市值
                            si.LiuTongShiZhi = (((((child.children[1] as IHTMLElement).children[1] as IHTMLElement)).children[3] as IHTMLElement).children[1] as IHTMLElement).innerText.Replace("亿", string.Empty); // 流通市值

                            si.HuanShouLv = (((((child.children[1] as IHTMLElement).children[2] as IHTMLElement)).children[0] as IHTMLElement).children[1] as IHTMLElement).innerText; // 换手率
                            si.ShiJingLv = (((((child.children[1] as IHTMLElement).children[2] as IHTMLElement)).children[1] as IHTMLElement).children[1] as IHTMLElement).innerText; // 市净率
                            si.ZhenFu = (((((child.children[1] as IHTMLElement).children[2] as IHTMLElement)).children[2] as IHTMLElement).children[1] as IHTMLElement).innerText; // 振幅
                            si.ShiYingLv = (((((child.children[1] as IHTMLElement).children[2] as IHTMLElement)).children[3] as IHTMLElement).children[1] as IHTMLElement).innerText; // 市盈率
                        }
                    }
                }
            }
            si.Timestamp = DateTime.Now;
            WriteData(si);
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
              new Action(delegate { }));
        }
    }
}
