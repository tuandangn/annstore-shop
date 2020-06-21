using System.Linq;
using System.Threading.Tasks;

namespace Annstore.Query
{
    public interface IQueryRepository<TEntity> where TEntity : QueryBaseEntity
    {
        Task<TEntity> FindByIdAsync(string id);

        IQueryable<TEntity> GetAll();
    }
}
