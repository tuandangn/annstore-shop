using Annstore.Core.Entities.Catalog;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Annstore.Core.Entities.Users;

namespace Annstore.Data
{
    public class AnnstoreDbContext : IdentityDbContext<AppUser, AppRole, int>, IDbContext
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
