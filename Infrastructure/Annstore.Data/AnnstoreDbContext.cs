using Microsoft.EntityFrameworkCore;

namespace Annstore.Data
{
    public sealed class AnnstoreDbContext : DbContext, IDbContext
    {
        public AnnstoreDbContext(DbContextOptions<AnnstoreDbContext> opts) : base(opts)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnnstoreDbContext).Assembly);
        }
    }
}
