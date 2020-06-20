using System.Linq;
using System.Threading.Tasks;
using Annstore.Core;

namespace Annstore.Data
{
    public class RepositoryBase<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IDbContext _dbContext;

        public RepositoryBase(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> Table => _dbContext.Set<T>();

        public ValueTask<T> FindByIdAsync(int id)
        {
            return _dbContext.FindAsync<T>(id);
        }

        public async virtual ValueTask<T> InsertAsync(T entity)
        {
            var result = await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            return result.Entity;
        }

        public async virtual ValueTask<T> UpdateAsync(T entity)
        {
            var result = _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);

            return result.Entity;
        }

        public async virtual Task DeleteAsync(T entity)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync()
                .ConfigureAwait(false);
        }
    }
}
