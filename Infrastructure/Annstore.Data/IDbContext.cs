using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Annstore.Data
{
    public interface IDbContext
    {
        TEntity Find<TEntity>(params object[] keyValues) where TEntity : class;

        ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;

        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;

        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class;

        void AddRange(IEnumerable<object> entities);

        Task AddRangeAsync(
            IEnumerable<object> entities,
            CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;

        EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();

        Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
