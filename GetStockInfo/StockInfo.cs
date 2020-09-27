using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetStockInfo
{
    public class StockInfo
    {
        /// <summary>
        /// 股票名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 股票代码
        /// </summary>
        public StockCode Code { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 涨幅点数
        /// </summary>
        public string ZhangFuDianShu { get; set; }
        /// <summary>
        /// 涨幅比率
        /// </summary>
        public string ZhangFuBiLv { get; set; }
        /// <summary>
        /// 昨收
        /// </summary>
        public string ZuoShou { get; set; }
        /// <summary>
        /// 今开
        /// </summary>
        public string JinKai { get; set; }
        /// <summary>
        /// 最高
        /// </summary>
        public string ZuiGao { get; set; }
        /// <summary>
        /// 最低
        /// </summary>
        public string ZuiDi { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public string ChengJiaoLiang { get; set; }
        /// <summary>
        /// 成交额
        /// </summary>
        public string ChengJiaoE { get; set; }
        /// <summary>
        /// 总市值
        /// </summary>
        public string ZongShiZhi { get; set; }
        /// <summary>
        /// 流通市值
        /// </summary>
        public string LiuTongShiZhi { get; set; }
        /// <summary>
        /// 换手率
        /// </summary>
        public string HuanShouLv { get; set; }
        /// <summary>
        /// 市净率
        /// </summary>
        public string ShiJingLv { get; set; }
        /// <summary>
        /// 振幅
        /// </summary>
        public string ZhenFu { get; set; }
        /// <summary>
        /// 市盈率
        /// </summary>
        public string ShiYingLv { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }


        public virtual string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} {Code}  {Price}  {ZhangFuDianShu}  {ZhangFuBiLv}");
            sb.AppendLine($"昨收：{ZuoShou}  今开：{JinKai}  最高：{ZuiGao}  最低：{ZuiDi}");
            sb.AppendLine($"成交量：{ChengJiaoLiang}手  成交额：{ChengJiaoE}亿  总市值：{ZongShiZhi}  流通市值：{LiuTongShiZhi}");
            sb.AppendLine($"换手率：{HuanShouLv}  市净率：{ShiJingLv}  振幅：{ZhenFu}  市盈率：{ShiYingLv}");
            Console.WriteLine(sb);
            return sb.ToString();
        }
    }

    public enum Exchange
    {
        SH,
        SZ,
    }

    public class StockCode
    {
        private StockCode() { }

        public string Code { get; set; }
        public Exchange Exchange { get; set; }

        public override string ToString()
        {
            var _ = Exchange == Exchange.SH ? "SH" : "SZ";
            return $"{Code}.{_}";
        }

        public static StockCode Parse(string strCode)
        {
            if (strCode.Contains("."))
            {
                var code = new StockCode();
                code.Code = strCode.Substring(0, strCode.IndexOf('.'));
                code.Exchange = strCode.Substring(strCode.IndexOf('.') + 1) == "SH" ? Exchange.SH : Exchange.SZ;
                return code;
            }
            return null;
        }
    }
}
