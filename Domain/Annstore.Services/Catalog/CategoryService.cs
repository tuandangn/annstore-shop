using System;
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

        public async ValueTask<List<Category>> GetCategoriesAsync()
        {
            var query = from category in _categoryRepository.Table
                        select category;

            var result = await query.ToListAsync().ConfigureAwait(false);

            return result;
        }

        public async ValueTask<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.FindByIdAsync(id).ConfigureAwait(false);

            return category;
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var result = await _categoryRepository.UpdateAsync(category).ConfigureAwait(false);

            return result;
        }
    }
}
