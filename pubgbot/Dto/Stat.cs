using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pubgbot.Dto
{
    /// <summary>
    /// Stats per RegionStats
    /// </summary>
    public class Stat
    {
        public object partition { get; set; }
        public string label { get; set; }
        public object subLabel { get; set; }
        public string field { get; set; }
        public string category { get; set; }
        public int? ValueInt { get; set; }
        public double? ValueDec { get; set; }
        public string value { get; set; }
        public int? rank { get; set; }
        public double percentile { get; set; }
        public string displayValue { get; set; }
    }
}
