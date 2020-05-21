using Annstore.Core.Entities.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Infrastructure;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Factories
{
    public interface IAdminCategoryService
    {
        Task<CategoryModel> GetCategoryModelAsync(int id);

        Task<CategoryListModel> GetCategoryListModelAsync();

        Task<CategoryModel> PrepareCategoryModelParentCategoriesAsync(CategoryModel model);

        Task<AppResponse<Category>> CreateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse<Category>> UpdateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse> DeleteCategoryAsync(AppRequest<int> request);
    }
}
