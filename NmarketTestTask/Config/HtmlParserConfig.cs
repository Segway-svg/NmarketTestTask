using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmarketTestTask.Config
{
    public static class HtmlParserConfig
    {
        public static string TheadNode { get; set; } = "//thead";
        public static string ThNode { get; set; } = ".//th";
        public static string TbodyNode { get; set; } = "//tbody/tr";
        public static string TrNode { get; set; } = "//tr[td]";
        public static string Currency { get; set; } = " руб.";
        public static string DefaultPrice { get; set; } = "0";
    }
}
