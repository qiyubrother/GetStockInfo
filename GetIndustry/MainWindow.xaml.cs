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
using System.Net;
using System.Net.Http.Headers;
using System.Data.SqlClient;
using Database;
using AhDung.WinForm;
namespace GetIndustry
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

        public MainWindow()
        {
            InitializeComponent();

            var s = File.ReadAllText(System.IO.Path.Combine("config.json"));
            JObject jo = (JObject)JsonConvert.DeserializeObject(s);
            SqlServer.ConnectionString = jo["ConnectionString"].ToString();
            SqlServer.Instance.ExcuteSQL($"delete from StockIndustry where Industry = ''"); // 删除无效数据
            var dt = SqlServer.Instance.GetDataTable($"select * from codes as a where a.code not in (select code from StockIndustry as b)");
            if (dt.Rows.Count == 0)
            {
                OutputDebugString($"Empty data.");
                Application.Current.Shutdown();
                return;
            }
            
            foreach (DataRow dr in dt.Rows)
            {
                codeList.Add($"{dr["Exchange"].ToString()}{dr["Code"].ToString()}");
            }

            SuppressScriptErrors(web, true);
            web.Navigated += Navigated;
            Loaded += (sender, e) => GetIndustryInfo(codeList);
        }

        public void Navigated(object sender, NavigationEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Compute();
                }
                catch (Exception ex)
                {
                    OutputDebugString($"Compute.Exception::{ex.Message}");
                }
                pos++;
                if (pos < codeList.Count)
                {
                    web.Dispose();
                    web = new WebBrowser();
                    web.Navigated += Navigated;
                    grid.Children.Add(web);
                    GetIndustryInfo(codeList);
                }
                else
                {
                    Close();
                }
            }));
        }

        private void WriteData(LiteStockInfo data)
        {
            Console.WriteLine($"[{pos}/{codeList.Count}]{data.Exchange}{data.Code}, {data.Name}, {data.Industry}");
            try
            {
                SqlServer.Instance.ExcuteSQL(new[]
                {
                    new SqlCommand($"delete from StockIndustry where Code = '{data.Code}' and Exchange ='{data.Exchange}'"),
                    new SqlCommand($"INSERT INTO StockIndustry(Code, Exchange, Name, Industry, ROE, IndustryROE, href) VALUES ('{data.Code}', '{data.Exchange}', '{data.Name}', '{data.Industry}', {data.ROE}, {data.IndustryROE}, '{data.Href}')")
                });
            }
            catch(Exception ex) { Console.WriteLine($"WriteData.Exception::{ex.Message}"); }
        }

        public void GetIndustryInfo(List<string> codeList)
        {
            web.Navigate($"http://quote.eastmoney.com/{codeList[pos]}.html?_={DateTime.Now.Ticks}");
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
            var name = doc.getElementById("name") == null ? string.Empty : doc.getElementById("name").innerText;

            IHTMLElement mainPanel = doc.getElementById("cwzbDataBox");

            LiteStockInfo si = new LiteStockInfo();

            if (mainPanel == null)
            {
                // 读取错误。
                Console.WriteLine("读取错误。");
                return;
            }
            int retryCount = 0;
            var industry = string.Empty;
            var roe = string.Empty;
            var industryROE = string.Empty;
            var href = "#";
retry:
            try
            {
                Thread.Sleep(50);
                industry = (((mainPanel.children[1] as IHTMLElement).children[0] as IHTMLElement).children[0] as IHTMLElement).innerText; // 行业
                roe = ((mainPanel.children[0] as IHTMLElement).children[8] as IHTMLElement).innerText; // ROE
                industryROE = ((mainPanel.children[1] as IHTMLElement).children[8] as IHTMLElement).innerText; // 行业平均ROE
                href = (((mainPanel.children[1] as IHTMLElement).children[0] as IHTMLElement).children[0] as IHTMLElement).getAttribute("href"); // href
                if (industry == string.Empty)
                {
                    if (retryCount <= 5)
                    {
                        Thread.Sleep(100);
                        retryCount++;
                        goto retry;
                    }
                }
            }
            catch
            {
                if (retryCount <= 5)
                {
                    retryCount++;
                    goto retry;
                }
            }
            si.Name = name;
            si.Code = codeList[pos].Substring(2);
            si.Exchange = codeList[pos].Substring(0, 2);
            si.Industry = industry.Replace("%", string.Empty);
            decimal.TryParse(roe.Replace("%", string.Empty), out decimal _roe);
            si.ROE = _roe;
            decimal.TryParse(industryROE.Replace("%", string.Empty), out decimal _industryROE);
            si.IndustryROE = _industryROE;
            si.Href = href;

            WriteData(si);
        }
    }

    sealed class LiteStockInfo
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
        /// 股票名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 行业
        /// </summary>
        public string Industry { get; set; }
        /// <summary>
        /// 行业ROE
        /// </summary>
        public decimal IndustryROE { get; set; }
        /// <summary>
        /// ROE
        /// </summary>
        public decimal ROE { get; set; }
        /// <summary>
        /// Href
        /// </summary>
        public string Href { get; set; }
    }
}
