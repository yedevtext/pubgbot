using PUBGSharp.Data;

namespace pubgbot.dbcontext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("User")]
    public partial class User
    {
        public int Id { get; set; }

        [StringLength(150)]
        public string DiscordName { get; set; }

        [StringLength(150)]
        public string SteamId { get; set; }

       
        public Region Region { get; set; }
    }
}
