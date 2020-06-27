using System.Linq;
using System.Threading.Tasks;

namespace Annstore.DataMixture
{
    public interface IMixRepository<TEntity>
    {
        Task<TEntity> FindByIdAsync(string id);

        Task<TEntity> FindByEntityIdAsync(int id);

        Task<TEntity> InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        IQueryable<TEntity> GetAll();
    }
}
