using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Common;
using Annstore.Core.Entities.Catalog;

namespace Annstore.Services.Catalog
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryByIdAsync(int id);

        Task<List<Category>> GetCategoriesAsync();

        Task<IPagedList<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize);

        Task<Category> UpdateCategoryAsync(Category category);

        Task<Category> CreateCategoryAsync(Category category);

        Task DeleteCategoryAsync(Category category);

        Task<List<Category>> GetCategoryBreadcrumbAsync(Category category, int deepLevel);
    }
}
