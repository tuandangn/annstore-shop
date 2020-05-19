using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;

namespace Annstore.Services.Catalog
{
    public interface ICategoryService
    {
        ValueTask<Category> GetCategoryByIdAsync(int id);

        ValueTask<List<Category>> GetCategoriesAsync();

        Task<Category> UpdateCategory(Category category);
    }
}
