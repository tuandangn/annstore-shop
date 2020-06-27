using Annstore.Core.Entities.Catalog;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public interface ICategoryDataDependencyResolver
    {
        Task<MixCategory> CreateMixCategoryForCategoryAsync(Category category);

        Task UpdateChildrenOfMixParentCategoryOfAsync(Category category);

        Task UpdateChildrenOfMixParentCategoryForAsync(MixCategory mixCategory);

        Task UpdateBreadcrumbsOfCategoryAsync(Category category);

        Task<MixCategory> UpdateMixCategoryForCategoryAsync(Category category);

        Task DeleteMixCategoryForCategoryAsync(Category category);
    }
}
