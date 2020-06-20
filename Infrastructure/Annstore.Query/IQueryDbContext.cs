using System.Linq;
using System.Threading.Tasks;

namespace Annstore.Query
{
    public interface IQueryDbContext
    {
        TEntity Find<TEntity>(params object[] keyValues) where TEntity : class;

        ValueTask<TEntity> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;
    }
}
