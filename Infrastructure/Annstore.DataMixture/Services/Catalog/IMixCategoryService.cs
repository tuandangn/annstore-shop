using Annstore.Core.Entities.Catalog;
using System.Threading.Tasks;

namespace Annstore.DataMixture.Services.Catalog
{
    public interface IMixCategoryService
    {
        Task CreateMixCategoryAsync(Category category);

        Task DeleteMixCategoryAsync(Category category);

        Task UpdateMixCategoryAsync(Category category);
    }
}
