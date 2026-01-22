using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NmarketTestTask.Config
{
    public static class ExcelParserConfig
    {
        public static string HouseMarker { get; set; } = "Дом";
        public static string FlatMarker { get; set; } = "№";
        public static string DefaultPriceValue { get; set; } = "0";
    }
}
