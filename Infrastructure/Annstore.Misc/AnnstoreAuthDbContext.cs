using Annstore.Auth.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Auth
{
    public sealed class AnnstoreAuthDbContext : IdentityDbContext<Account, Role, int>
    {
        public AnnstoreAuthDbContext(DbContextOptions<AnnstoreAuthDbContext> opts) : base(opts)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnnstoreAuthDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
