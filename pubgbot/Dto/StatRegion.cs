using System.Collections.Generic;

namespace pubgbot.Dto
{
    /// <summary>
    /// Region
    /// </summary>
    public class StatRegion
    {
        public string Region { get; set; }
        public string Season { get; set; }
        public string Match { get; set; }
        public List<Stat> Stats { get; set; }
    }
}