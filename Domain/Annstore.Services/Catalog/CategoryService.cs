using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annstore.Core.Common;
using Annstore.Core.Entities.Catalog;
using Annstore.Data.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Services.Catalog
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<Category>> GetCategoriesAsync(bool showHidden = false)
        {
            var query = from category in _categoryRepository.Table
                        where !category.Deleted && (showHidden || category.Published)
                        orderby category.Id descending
                        select category;

            var result = await query.ToListAsync()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.FindByIdAsync(id)
                .ConfigureAwait(false);

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var result = await _categoryRepository.UpdateAsync(category)
                .ConfigureAwait(false);
            return result;

        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var result = await _categoryRepository.InsertAsync(category)
                .ConfigureAwait(false);

            return result;
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await _categoryRepository.DeleteAsync(category)
                .ConfigureAwait(false);
        }

        public async Task<List<Category>> GetCategoryBreadcrumbAsync(Category category, int deepLevel, bool showHidden = false)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (deepLevel < 0)
                throw new ArgumentException("Deep level cannot less than 0");

            var breadcrumb = new List<Category> { category };
            if (deepLevel == 0 || category.ParentId == 0)
                return breadcrumb;

            await _GetCategoryBreadcrum(breadcrumb, category, 0, deepLevel, showHidden)
                .ConfigureAwait(false);
            breadcrumb.Reverse();

            return breadcrumb;
        }

        private async Task<List<Category>> _GetCategoryBreadcrum(List<Category> breadcrumb, Category category, int currentLevel, int maxLevel, bool showHidden = false)
        {
            if (category.ParentId == 0 || currentLevel == maxLevel)
                return breadcrumb;

            var parentCategory = await _categoryRepository.FindByIdAsync(category.ParentId)
                .ConfigureAwait(false);
            if (parentCategory == null || parentCategory.Deleted || (!showHidden && !parentCategory.Published))
                return breadcrumb;

            breadcrumb.Add(parentCategory);
            await _GetCategoryBreadcrum(breadcrumb, parentCategory, currentLevel + 1, maxLevel, showHidden)
                .ConfigureAwait(false);

            return breadcrumb;
        }

        public async Task<IPagedList<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize, bool showHidden = false)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must greater than or equal 1");
            if (pageSize <= 0)
                throw new ArgumentException("Page size must greater than 0");

            var query = from category in _categoryRepository.Table
                        where !category.Deleted && (showHidden || category.Published)
                        orderby category.Id descending
                        select category;
            //*TODO*
            var allCategories = await query.ToListAsync()
                .ConfigureAwait(false);
            var categories = allCategories.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var result = categories.ToPagedList(pageSize, pageNumber, allCategories.Count);

            return result;
        }

        public async Task<List<Category>> GetChildrenCategoriesAsync(Category parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var query = from category in _categoryRepository.Table
                        where !category.Deleted && category.Published &&
                            category.ParentId == parent.Id
                        orderby category.Id descending
                        select category;

            return await query.ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
