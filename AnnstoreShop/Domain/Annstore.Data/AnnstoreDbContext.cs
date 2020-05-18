using Annstore.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Data
{
    public class AnnstoreDbContext : DbContext
    {
        public AnnstoreDbContext(DbContextOptions<AnnstoreDbContext> opts) : base(opts)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnnstoreDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Category> Categories { get; set; }
    }
}
