namespace pubgbot.dbcontext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Stat")]
    public partial class Stat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [StringLength(150)]
        public string SteamId { get; set; }

        [StringLength(150)]
        public string Region { get; set; }

        [StringLength(150)]
        public string Category { get; set; }

        [StringLength(150)]
        public string Field { get; set; }

        [StringLength(150)]
        public string DisplayValue { get; set; }

        [StringLength(150)]
        public string Value { get; set; }

        public int? ValueInt { get; set; }

        public decimal? ValueDec { get; set; }

        public int? Rank { get; set; }

        public decimal? Percentile { get; set; }
    }
}
