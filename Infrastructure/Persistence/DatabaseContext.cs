using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class DatabaseContext : DbContext
    {
        public DbSet<DbPlanning> Plannings { get; set; }
        public DbSet<DbPayment> Payments { get; set; }
        public DbSet<DbTreatment> Treatments { get; set; }
        public DbSet<DbLocationTreatment> LocationTreatments { get; set; }

        public DatabaseContext() : base() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbLocationTreatment>()
                .HasKey(lt => new { lt.Location, lt.TreatmentType });
        }
    }
}
