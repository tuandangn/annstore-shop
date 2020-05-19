using System.Linq;
using System.Threading.Tasks;
using Annstore.Core;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Data
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IDbContext _dbContext;

        public Repository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> Table => _dbContext.Set<T>();

        public ValueTask<T> FindByIdAsync(int id)
        {
            return _dbContext.FindAsync<T>(id);
        }

        public async ValueTask<T> InsertAsync(T entity)
        {
            var result = await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async ValueTask<T> UpdateAsync(T entity)
        {
            var result = _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
