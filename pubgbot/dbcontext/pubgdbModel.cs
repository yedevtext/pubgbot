namespace pubgbot.dbcontext
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class pubgdbModel : DbContext
    {
        public pubgdbModel()
            : base("name=pubgdbModel")
        {
        }

        public virtual DbSet<Stat> Stats { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stat>()
                .Property(e => e.ValueDec)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Stat>()
                .Property(e => e.Percentile)
                .HasPrecision(18, 0);
        }
    }
}
