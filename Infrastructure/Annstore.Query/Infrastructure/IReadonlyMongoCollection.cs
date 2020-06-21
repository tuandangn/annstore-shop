using MongoDB.Driver;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Annstore.Query.Infrastructure
{
    public interface IReadonlyMongoCollection<TEntity>
    {
        Task<IAsyncCursor<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter);

        IQueryable<TEntity> AsQueryable();
    }
}
