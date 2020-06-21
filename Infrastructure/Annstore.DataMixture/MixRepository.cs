using Annstore.Query;
using Annstore.Query.Infrastructure;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Annstore.DataMixture
{
    public sealed class MixRepository<TEntity> : IMixRepository<TEntity> where TEntity : QueryBaseEntity
    {
        private static readonly string _collectionName;

        private readonly IMongoCollection<TEntity> _collection;

        static MixRepository()
        {
            _collectionName = typeof(TEntity).Name;
        }

        public MixRepository(IQueryDbSettings settings)
        {
            _collection = GetCollection(settings);
        }

        private IMongoCollection<TEntity> GetCollection(IQueryDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            var collection = database.GetCollection<TEntity>(_collectionName);
            return collection;
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await _collection.DeleteOneAsync(item => item.Id == entity.Id).ConfigureAwait(false);
        }

        public async Task<TEntity> FindByIdAsync(string id)
        {
            var findResult = await _collection.FindAsync(entity => entity.Id == id).ConfigureAwait(false);
            return await findResult.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            return _collection.FindOneAndReplaceAsync(item => item.Id == entity.Id, entity);
        }

        public async Task<TEntity> FindByEntityIdAsync(int entityId)
        {
            var findResult = await _collection.FindAsync(entity => entity.EntityId == entityId).ConfigureAwait(false);
            return await findResult.FirstOrDefaultAsync().ConfigureAwait(false);
        }
    }
}
