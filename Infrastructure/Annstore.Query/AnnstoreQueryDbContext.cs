using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Annstore.Query
{
    public sealed class AnnstoreQueryDbContext : IQueryDbContext
    {
        private readonly DbContext _dbContext;

        public AnnstoreQueryDbContext(QueryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            return _dbContext.Find<TEntity>(keyValues);
        }

        public ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            return _dbContext.FindAsync<TEntity>(keyValues);
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }

        public sealed class QueryDbContext : DbContext
        {
            public QueryDbContext(DbContextOptions<QueryDbContext> opts) : base(opts) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.HasDefaultSchema("query");
                modelBuilder.ApplyConfigurationsFromAssembly(typeof(QueryDbContext).Assembly);
            }
        }
    }
}
