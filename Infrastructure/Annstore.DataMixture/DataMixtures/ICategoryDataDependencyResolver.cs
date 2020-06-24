using Annstore.Core.Entities.Catalog;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public interface ICategoryDataDependencyResolver
    {
        Task<MixCategory> CreateMixCategoryForCategoryAsync(Category category);

        Task UpdateChildrenOfMixParentCategoryAsync(Category category);

        Task DeleteMixCategoryForCategoryAsync(Category category);

        Task<MixCategory> UpdateMixCategoryForCategoryAsync(Category category);
    }
}
