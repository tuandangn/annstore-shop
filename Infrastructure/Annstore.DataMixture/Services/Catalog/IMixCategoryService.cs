using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Services.Catalog
{
    public interface IMixCategoryService
    {
        Task<MixCategory> GetMixCategoryByEntityIdAsync(int id);

        Task<MixCategory> CreateMixCategoryAsync(MixCategory category);

        Task DeleteMixCategoryAsync(MixCategory category);

        Task<MixCategory> UpdateMixCategoryAsync(MixCategory category);
    }
}
