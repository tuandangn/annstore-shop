using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Common;
using Annstore.Core.Entities.Catalog;

namespace Annstore.Services.Catalog
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryByIdAsync(int id);

        Task<List<Category>> GetCategoriesAsync(bool showHidden = false);

        Task<IPagedList<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize, bool showHidden = false);

        Task<Category> UpdateCategoryAsync(Category category);

        Task<Category> CreateCategoryAsync(Category category);

        Task DeleteCategoryAsync(Category category);

        Task<List<Category>> GetCategoryBreadcrumbAsync(Category category, int deepLevel, bool showHidden = false);

        Task<List<Category>> GetChildrenCategoriesAsync(Category category);
    }
}
