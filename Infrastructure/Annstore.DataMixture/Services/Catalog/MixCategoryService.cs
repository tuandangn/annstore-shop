using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QueryCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Services.Catalog
{
    public class MixCategoryService : IMixCategoryService
    {
        private readonly ICategoryService _categoryService;
        private readonly IMixRepository<QueryCategory> _mixCategoryRepository;
        private readonly IMapper _mapper;

        public MixCategoryService(ICategoryService categoryService, IMixRepository<QueryCategory> mixCategoryRepository, IMapper mapper)
        {
            _categoryService = categoryService;
            _mixCategoryRepository = mixCategoryRepository;
            _mapper = mapper;
        }

        public async Task CreateMixCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await InsertMixCategory(category);
            await UpdateChildrenOfMixParentCategory(category);
        }

        public async Task DeleteMixCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(Category));

            var queryCategory = await _mixCategoryRepository.FindByEntityIdAsync(category.Id);
            if (queryCategory == null)
                return;

            await _mixCategoryRepository.DeleteAsync(queryCategory);
            await UpdateChildrenOfMixParentCategory(category);
        }

        private async Task InsertMixCategory(Category category)
        {
            var queryCategory = _mapper.Map<QueryCategory>(category);
            await _mixCategoryRepository.InsertAsync(queryCategory);
        }

        private async Task UpdateChildrenOfMixParentCategory(Category category)
        {
            if (category.ParentId == 0)
                return;

            var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentId);
            if (parentCategory == null || parentCategory.Deleted || !parentCategory.Published)
                return;

            var parentMixCategory = await _mixCategoryRepository.FindByEntityIdAsync(category.ParentId);
            if (parentMixCategory == null)
                return;

            parentMixCategory.Children = await GetChildrenMixCategoriesOf(parentCategory);
            await _mixCategoryRepository.UpdateAsync(parentMixCategory);
        }

        private async Task<IList<QueryCategory>> GetChildrenMixCategoriesOf(Category category)
        {
            var childrenCategories = await _categoryService.GetChildrenCategoriesAsync(category);
            var childrenMixCategories = new List<QueryCategory>();
            foreach (var child in childrenCategories)
            {
                var childCategoryModel = _mapper.Map<QueryCategory>(child);
                childrenMixCategories.Add(childCategoryModel);
            }
            return childrenMixCategories;
        }

        public async Task UpdateMixCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var queryCategory = await _mixCategoryRepository.FindByEntityIdAsync(category.Id);
            if (queryCategory == null)
                return;

            queryCategory = _mapper.Map<Category, QueryCategory>(category, queryCategory);
            await _mixCategoryRepository.UpdateAsync(queryCategory);
            await UpdateChildrenOfMixParentCategory(category);
        }
    }
}
