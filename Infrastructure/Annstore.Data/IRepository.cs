using System.Linq;
using System.Threading.Tasks;
using Annstore.Core;

namespace Annstore.Data
{
    public interface IRepository<T> where T : class, IAggregateRoot
    {
        IQueryable<T> Table { get; }

        ValueTask<T> FindByIdAsync(int id);

        ValueTask<T> InsertAsync(T entity);

        ValueTask<T> UpdateAsync(T entity);

        Task DeleteAsync(T entity);
    }
}
