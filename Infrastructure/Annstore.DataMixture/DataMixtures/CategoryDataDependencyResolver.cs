using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.Services.Catalog;
using Annstore.Services.Catalog;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public sealed class CategoryDataDependencyResolver : ICategoryDataDependencyResolver
    {
        private readonly ICategoryService _categoryService;
        private readonly IMixCategoryService _mixCategoryService;
        private readonly IMapper _mapper;

        public CategoryDataDependencyResolver(ICategoryService categoryService, IMixCategoryService mixCategoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mixCategoryService = mixCategoryService;
            _mapper = mapper;
        }

        public async Task<MixCategory> CreateMixCategoryForCategoryAsync(Core.Entities.Catalog.Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var mixCategory = _mapper.Map<MixCategory>(category);
            mixCategory = await _mixCategoryService.CreateMixCategoryAsync(mixCategory)
                .ConfigureAwait(false);

            return mixCategory;
        }

        public async Task DeleteMixCategoryForCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var mixCategory = await _mixCategoryService.GetMixCategoryByEntityIdAsync(category.Id)
                .ConfigureAwait(false);
            if (mixCategory == null)
                return;
            await _mixCategoryService.DeleteMixCategoryAsync(mixCategory)
                .ConfigureAwait(false);
        }

        public async Task UpdateChildrenOfMixParentCategoryAsync(Core.Entities.Catalog.Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (category.ParentId == 0)
                return;

            var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentId)
                .ConfigureAwait(false);
            if (parentCategory == null || parentCategory.Deleted || !parentCategory.Published)
                return;

            var parentMixCategory = await _mixCategoryService.GetMixCategoryByEntityIdAsync(category.ParentId)
                .ConfigureAwait(false);
            if (parentMixCategory == null)
                parentMixCategory = await CreateMixCategoryForCategoryAsync(parentCategory)
                    .ConfigureAwait(false);

            parentMixCategory.Children = await GetChildrenMixCategoriesOf(parentCategory)
                .ConfigureAwait(false);
            await _mixCategoryService.UpdateMixCategoryAsync(parentMixCategory)
                .ConfigureAwait(false);
        }

        public async Task<MixCategory> UpdateMixCategoryForCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var mixCategory = await _mixCategoryService.GetMixCategoryByEntityIdAsync(category.Id)
                .ConfigureAwait(false);
            if (mixCategory == null)
            {
                mixCategory = await CreateMixCategoryForCategoryAsync(category);
            }
            else
            {
                mixCategory = _mapper.Map<Category, MixCategory>(category, mixCategory);
                await _mixCategoryService.UpdateMixCategoryAsync(mixCategory)
                    .ConfigureAwait(false);
            }

            return mixCategory;
        }

        private async Task<IList<MixCategory>> GetChildrenMixCategoriesOf(Core.Entities.Catalog.Category category)
        {
            var childrenCategories = await _categoryService.GetChildrenCategoriesAsync(category)
                .ConfigureAwait(false);
            var childrenMixCategories = new List<MixCategory>();
            foreach (var child in childrenCategories)
            {
                var childCategoryModel = _mapper.Map<MixCategory>(child);
                childrenMixCategories.Add(childCategoryModel);
            }

            return childrenMixCategories;
        }
    }
}
