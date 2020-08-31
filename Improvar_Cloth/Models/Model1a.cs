namespace Improvar.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1a : DbContext
    {
        public Model1a()
            : base("name=Model1a")
        {
        }

        public virtual DbSet<M_GROUP> M_GROUP { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.CLCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.DTAG)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.TTAG)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.ITGRPCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.ITGRPNM)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.GRPNM)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.ITGRPTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.HSNCODE)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.PRODGRPCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.BARGENTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.SHORTNM)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.SALGLCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.PURGLCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.SALRETGLCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.PURRETGLCD)
                .IsUnicode(false);

            modelBuilder.Entity<M_GROUP>()
                .Property(e => e.CLASS1CD)
                .IsUnicode(false);
        }
    }
}
