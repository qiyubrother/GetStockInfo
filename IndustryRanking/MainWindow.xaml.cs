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
using Database;
using System.Data.SqlClient;
namespace IndustryRanking
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
        private List<string> industryList = new List<string>();
        public MainWindow()
        {
            InitializeComponent();

            SuppressScriptErrors(web, true);

            industryList = new List<string>
            {
                "http://quote.eastmoney.com/bk/90.BK0537.html",
                "http://quote.eastmoney.com/bk/90.BK0546.html",
                "http://quote.eastmoney.com/bk/90.BK0457.html",
                "http://quote.eastmoney.com/bk/90.BK0459.html",
                "http://quote.eastmoney.com/bk/90.BK0458.html",
                "http://quote.eastmoney.com/bk/90.BK0910.html",
                "http://quote.eastmoney.com/bk/90.BK0538.html",
                "http://quote.eastmoney.com/bk/90.BK0739.html",
                "http://quote.eastmoney.com/bk/90.BK0480.html",
                "http://quote.eastmoney.com/bk/90.BK0481.html",
                "http://quote.eastmoney.com/bk/90.BK0440.html",
                "http://quote.eastmoney.com/bk/90.BK0447.html",
                "http://quote.eastmoney.com/bk/90.BK0539.html",
                "http://quote.eastmoney.com/bk/90.BK0436.html",
                "http://quote.eastmoney.com/bk/90.BK0727.html",
                "http://quote.eastmoney.com/bk/90.BK0737.html",
                "http://quote.eastmoney.com/bk/90.BK0428.html",
                "http://quote.eastmoney.com/bk/90.BK0545.html",
                "http://quote.eastmoney.com/bk/90.BK0740.html",
                "http://quote.eastmoney.com/bk/90.BK0437.html",
                "http://quote.eastmoney.com/bk/90.BK0454.html",
                "http://quote.eastmoney.com/bk/90.BK0465.html",
                "http://quote.eastmoney.com/bk/90.BK0478.html",
                "http://quote.eastmoney.com/bk/90.BK0733.html",
                "http://quote.eastmoney.com/bk/90.BK0448.html",
                "http://quote.eastmoney.com/bk/90.BK0433.html",
                "http://quote.eastmoney.com/bk/90.BK0464.html",
                "http://quote.eastmoney.com/bk/90.BK0728.html",
                "http://quote.eastmoney.com/bk/90.BK0422.html",
                "http://quote.eastmoney.com/bk/90.BK0735.html",
                "http://quote.eastmoney.com/bk/90.BK0725.html",
                "http://quote.eastmoney.com/bk/90.BK0470.html",
                "http://quote.eastmoney.com/bk/90.BK0730.html",
                "http://quote.eastmoney.com/bk/90.BK0456.html",
                "http://quote.eastmoney.com/bk/90.BK0471.html",
                "http://quote.eastmoney.com/bk/90.BK0738.html",
                "http://quote.eastmoney.com/bk/90.BK0438.html",
                "http://quote.eastmoney.com/bk/90.BK0726.html",
                "http://quote.eastmoney.com/bk/90.BK0429.html",
                "http://quote.eastmoney.com/bk/90.BK0450.html",
                "http://quote.eastmoney.com/bk/90.BK0731.html",
                "http://quote.eastmoney.com/bk/90.BK0484.html",
                "http://quote.eastmoney.com/bk/90.BK0479.html",
                "http://quote.eastmoney.com/bk/90.BK0424.html",
                "http://quote.eastmoney.com/bk/90.BK0474.html",
                "http://quote.eastmoney.com/bk/90.BK0736.html",
                "http://quote.eastmoney.com/bk/90.BK0476.html",
                "http://quote.eastmoney.com/bk/90.BK0425.html",
                "http://quote.eastmoney.com/bk/90.BK0729.html",
                "http://quote.eastmoney.com/bk/90.BK0482.html",
                "http://quote.eastmoney.com/bk/90.BK0732.html",
                "http://quote.eastmoney.com/bk/90.BK0427.html",
                "http://quote.eastmoney.com/bk/90.BK0486.html",
                "http://quote.eastmoney.com/bk/90.BK0451.html",
                "http://quote.eastmoney.com/bk/90.BK0734.html",
                "http://quote.eastmoney.com/bk/90.BK0421.html",
                "http://quote.eastmoney.com/bk/90.BK0473.html",
                "http://quote.eastmoney.com/bk/90.BK0477.html",
                "http://quote.eastmoney.com/bk/90.BK0475.html",
                "http://quote.eastmoney.com/bk/90.BK0420.html",
                "http://quote.eastmoney.com/bk/90.BK0485.html",
            };

            web.Navigated += (o, ex) => {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Compute();
                    }
                    catch (Exception e)
                    {

                    }
                    pos++;
                    if (pos < industryList.Count)
                    {
                        GetIndustryStockInfo(industryList);
                    }
                    else
                    {
                        Close();
                    }
                }));
            };
            Loaded += (sender, e) => GetIndustryStockInfo(industryList);
        }

        public void WriteData(IndustryInfo data)
        {
            var cmd = new SqlCommand($"INSERT INTO IndustryRanking(Code, Exchange, Name, Industry, Rank) VALUES (@Code, @Exchange, @Name, @Industry, @Rank)");
            cmd.Parameters.AddRange(new[] {
                new SqlParameter("@Code", data.Code),
                new SqlParameter("@Exchange", data.Exchange.ToString()),
                new SqlParameter("@Name", data.Name),
                new SqlParameter("@Industry", data.Industry),
                new SqlParameter("@Rank", data.Rank)
            });

            SqlServer.Instance.ExcuteSQL(new [] {
                new SqlCommand($"delete from IndustryRanking where Code = '{data.Code}' and Exchange='{data.Exchange}'"),
                cmd
            });
        }

        public void GetIndustryStockInfo(List<string> lst)
        {
            web.Navigate($"{lst[pos]}");
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
            var industry = string.Empty;
            Thread.Sleep(300);

            var doc = web.Document as mshtml.HTMLDocument;
            if (doc == null)
            {
                Console.WriteLine("doc is null.");
                return;
            }
            IHTMLElement mainPanel = doc.getElementById("sfw_table");

            if (mainPanel == null)
            {
                // 读取错误。
                Console.WriteLine("读取错误。");
                return;
            }
            var title = doc.getElementsByTagName("span");
            foreach (IHTMLElement child in title)
            {
                if (child.className != null && child.className.Contains("quote_title_0"))
                {
                    industry = child.innerText;
                }
            }

            IndustryInfo si = new IndustryInfo();

            si.Name =          ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[1] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r1CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[1] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r1CodeString.Substring(0, 2);
            si.Code = r1CodeString.Substring(2);
            si.Rank = 1;
            si.Industry = industry;
            WriteData(si);

            si.Name =          ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[2] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r2CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[2] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r2CodeString.Substring(0, 2);
            si.Code = r2CodeString.Substring(2);
            si.Rank = 2;
            WriteData(si);

            si.Name = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[3] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r3CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[3] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r3CodeString.Substring(0, 2);
            si.Code = r3CodeString.Substring(2);
            si.Rank = 3;
            WriteData(si);

            si.Name = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[4] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r4CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[4] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r4CodeString.Substring(0, 2);
            si.Code = r4CodeString.Substring(2);
            si.Rank = 4;
            WriteData(si);

            si.Name = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[5] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r5CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[5] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r5CodeString.Substring(0, 2);
            si.Code = r5CodeString.Substring(2);
            si.Rank = 5;
            WriteData(si);

            si.Name = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[6] as IHTMLElement).children[0] as IHTMLElement).innerText;
            var r6CodeString = ((((mainPanel.children[0] as IHTMLElement).children[0] as IHTMLElement).children[6] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href").ToString().Replace("http://quote.eastmoney.com/", string.Empty).Replace(".html", string.Empty);
            si.Exchange = r6CodeString.Substring(0, 2);
            si.Code = r6CodeString.Substring(2);
            si.Rank = 6;
            WriteData(si);
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
              new Action(delegate { }));
        }
    }

    public class IndustryInfo
    {
        /// <summary>
        /// 股票名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 股票代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public string Industry { get; set; }
        /// <summary>
        /// Exchange
        /// </summary>
        public int Rank { get; set; }
    }
}
