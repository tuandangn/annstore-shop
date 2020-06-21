using Annstore.Query.Infrastructure;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Annstore.Query
{
    public sealed class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : QueryBaseEntity
    {
        private static readonly string _collectionName;
        private readonly IQueryDbContext _dbContext;

        public QueryRepository(IQueryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        static QueryRepository()
        {
            _collectionName = GetCollectionName();
        }

        private static string GetCollectionName()
        {
            var entityType = typeof(TEntity);
            return entityType.Name;
        }

        private IReadonlyMongoCollection<TEntity> Collection
        {
            get
            {
                return _dbContext.Database.GetCollection<TEntity>(_collectionName);
            }
        }

        public async Task<TEntity> FindByIdAsync(string id)
        {
            var findResult = await Collection.FindAsync(entity => entity.Id == id).ConfigureAwait(false);
            var result = await findResult.FirstOrDefaultAsync().ConfigureAwait(false);
            return result;
        }

        public IQueryable<TEntity> GetAll()
        {
            return Collection.AsQueryable();
        }
    }
}
