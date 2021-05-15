using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Net;
using System.IO;

namespace GetStockCode
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Codes> lst = new List<Codes>(600000);
            //using (var conn = new SQLiteConnection(@"Data Source=.\stock.db;Version=3;UseUTF16Encoding=True;"))
            {
                //conn.Open();
                var exchange = "sz";
                for (var i = 002001; i < 002999; i++)
                {
                    var code = i.ToString().PadLeft(6, '0');
                    var s = GetHtmltxt($"http://hq.sinajs.cn/list={exchange}{code}");
                    var arr = s.Split('=');
                    if (arr[1] == "\"\";\n" || arr[1] == "FAILED")
                    {
                        // 不存在
                    }
                    else if (s.Length > 30)
                    {
                        // 存在

                        Console.WriteLine(code);
                        lst.Add(new Codes { Code = code, Exchange = exchange });
                    }
                }
            }

            var sw = new StreamWriter("data.sz.zx.txt");
            foreach (var s in lst)
            {
                sw.WriteLine($"{s.Code} {s.Exchange}");
            }
            sw.Close();
            Console.WriteLine("All of work finished.");
            Console.ReadLine();
        }

        public static string GetHtmltxt(string url)
        {
            string str;

            //方式一
            WebRequest web = WebRequest.Create(url);
            web.Method = "GET";
            HttpWebResponse httpWeb = (HttpWebResponse)web.GetResponse();
            Stream stream = httpWeb.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            str = reader.ReadToEnd();
            stream.Close();
            reader.Close();


            //方式二
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            //HttpWebRequest类继承于WebRequest，并没有自己的构造函数，需通过WebRequest的Creat方法 建立，并进行强制的类型转换           

            HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
            //通过HttpWebRequest的GetResponse()方法建立HttpWebResponse,强制类型转换

            Stream respStream = httpResp.GetResponseStream();
            //GetResponseStream()方法获取HTTP响应的数据流,并尝试取得URL中所指定的网页内容

            StreamReader respStreamReader = new StreamReader(respStream, Encoding.UTF8);
            //返回的内容是Stream形式的，所以可以利用StreamReader类获取GetResponseStream的内容，
            //并以 StreamReader类的Read方法依次读取网页源程序代码每一行的内容，直至行尾（读取的编码格式：UTF8）

            str = respStreamReader.ReadToEnd();

            respStream.Close();
            respStreamReader.Close();

            return str;
        }
    }

    sealed class Codes
    {
        public string Exchange { get; set; }
        public string Code { get; set; }

    }
}
