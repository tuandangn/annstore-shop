using Annstore.Core.Entities.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Areas.Admin.Services.Categories.Options;
using Annstore.Web.Infrastructure;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Services.Categories
{
    public interface IAdminCategoryService
    {
        Task<CategoryModel> GetCategoryModelAsync(int id, BreadcrumbOptions breadcrumbOpts);

        Task<CategoryListModel> GetCategoryListModelAsync(CategoryListOptions options);

        Task<CategoryModel> PrepareCategoryModelParentCategoriesAsync(CategoryModel model, BreadcrumbOptions breadcrumbOpts);

        Task<AppResponse<Category>> CreateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse<Category>> UpdateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse> DeleteCategoryAsync(AppRequest<int> request);

        Task<string> GetCategoryBreadcrumbStringAsync(Category category, int deepLevel, string separator, bool useParenAsTarget);
    }
}
