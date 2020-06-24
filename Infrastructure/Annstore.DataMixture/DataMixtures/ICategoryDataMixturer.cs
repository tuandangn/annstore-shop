using Annstore.Core.Entities.Catalog;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public interface ICategoryDataMixturer
    {
        Task<MixCategory> ApplyForCreatedCategoryAsync(Category category);

        Task<MixCategory> ApplyForUpdatedCategoryAsync(Category category);

        Task ApplyForDeletedCategoryAsync(Category category);

        Task ApplyForCreatedMixCategoryAsync(MixCategory mixCategory);

        Task ApplyForUpdatedMixCategoryAsync(MixCategory mixCategory);

        Task ApplyForDeletedMixCategoryAsync(MixCategory mixCategory);
    }
}
