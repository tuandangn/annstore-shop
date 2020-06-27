using MongoDB.Driver;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Annstore.Query.Infrastructure
{
    public sealed class ReadonlyMongoCollection<TEntity> : IReadonlyMongoCollection<TEntity>
    {
        private readonly IMongoCollection<TEntity> _collection;

        private ReadonlyMongoCollection(IMongoCollection<TEntity> collection)
        {
            _collection = collection;
        }

        public static ReadonlyMongoCollection<TEntity> CreateFrom(IMongoCollection<TEntity> collection)
        {
            return new ReadonlyMongoCollection<TEntity>(collection);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return _collection.AsQueryable();
        }
        
        public Task<IAsyncCursor<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter)
        {
            return _collection.FindAsync(filter);
        }
    }
}
