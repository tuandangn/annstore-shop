using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;
using Annstore.Data;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Services.Catalog
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryService(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async ValueTask<List<Category>> GetCategories()
        {
            var query = from category in _categoryRepository.Table
                        select category;

            var result = await query.ToListAsync().ConfigureAwait(false);

            return result;
        }
    }
}
