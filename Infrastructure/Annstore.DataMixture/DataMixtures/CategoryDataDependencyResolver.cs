using Annstore.Core.Common;
using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.Services.Catalog;
using Annstore.Query.Infrastructure;
using Annstore.Services.Catalog;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public sealed class CategoryDataDependencyResolver : ICategoryDataDependencyResolver
    {
        #region Fields
        private const int CHILDREN_DEEP_LEVEL = 2;
        private readonly ICategoryService _categoryService;
        private readonly IMixCategoryService _mixCategoryService;
        private readonly IMapper _mapper;
        private readonly IStringHelper _stringHelper;
        #endregion

        #region Ctor
        public CategoryDataDependencyResolver(
            ICategoryService categoryService, IMixCategoryService mixCategoryService,
            IMapper mapper, IStringHelper stringHelper)
        {
            _categoryService = categoryService;
            _mixCategoryService = mixCategoryService;
            _mapper = mapper;
            _stringHelper = stringHelper;
        }
        #endregion

        #region Methods

        public async Task<MixCategory> CreateMixCategoryForCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var mixCategory = ConvertCategoryToMixCategory(category);
            mixCategory.Breadcrumb = await GetMixCategoryBreadcrumb(category)
                .ConfigureAwait(false);
            mixCategory = await _mixCategoryService.CreateMixCategoryAsync(mixCategory)
                .ConfigureAwait(false);

            return mixCategory;
        }

        private MixCategory ConvertCategoryToMixCategory(Category source, MixCategory destination = null)
        {
            if (destination == null)
                destination = _mapper.Map<MixCategory>(source);
            else
                destination = _mapper.Map(source, destination);
            destination.NormalizedName = _stringHelper.TransformVietnameseToAscii(source.Name);
            return destination;
        }

        private async Task<IList<MixCategory>> GetMixCategoryBreadcrumb(Category category)
        {
            var categoryBreadcrumb = await _categoryService.GetCategoryBreadcrumbAsync(
                category, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false)
                .ConfigureAwait(false);
            var mixCategoryBreadcrumb = categoryBreadcrumb.Select(category => ConvertCategoryToMixCategory(category))
                .ToList();
            return mixCategoryBreadcrumb;
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

        public async Task UpdateChildrenOfMixParentCategoryForAsync(MixCategory mixCategory)
        {
            if (mixCategory == null)
                throw new ArgumentNullException(nameof(mixCategory));

            var category = await _categoryService.GetCategoryByIdAsync(mixCategory.EntityId)
                .ConfigureAwait(false);
            if (category == null)
                return;

            await UpdateChildrenOfMixParentCategoryOfAsync(category)
                .ConfigureAwait(false);
        }

        public async Task UpdateChildrenOfMixParentCategoryOfAsync(Category category)
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
            {
                parentMixCategory = await CreateMixCategoryForCategoryAsync(parentCategory)
                    .ConfigureAwait(false);
            }

            await GetChildrenMixCategories(parentMixCategory, parentCategory, 0, CHILDREN_DEEP_LEVEL)
                .ConfigureAwait(false);
            await _mixCategoryService.UpdateMixCategoryAsync(parentMixCategory)
                .ConfigureAwait(false);
        }

        private async Task GetChildrenMixCategories(MixCategory target, Category parent, int currentDeepLevel, int maxDeepLevel)
        {
            if (currentDeepLevel == maxDeepLevel)
                return;

            var children = await _categoryService.GetChildrenCategoriesAsync(parent)
                .ConfigureAwait(false);
            var childrenMixCategories = new List<MixCategory>();
            foreach (var child in children)
            {
                var childMixCategory = ConvertCategoryToMixCategory(child);
                await GetChildrenMixCategories(childMixCategory, child, currentDeepLevel + 1, maxDeepLevel)
                    .ConfigureAwait(false);
                childrenMixCategories.Add(childMixCategory);
            }

            target.Children = childrenMixCategories;
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
                mixCategory = ConvertCategoryToMixCategory(category, mixCategory);
                await _mixCategoryService.UpdateMixCategoryAsync(mixCategory)
                .ConfigureAwait(false);
            }

            return mixCategory;
        }

        public async Task UpdateBreadcrumbsOfCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var containingCategoryBreadcrumbMixCategoriesQuery =
                //*TODO*
                from mcategory in _mixCategoryService.GetAllMixCategories().ToList()
                let breadcrumb = mcategory.Breadcrumb
                where breadcrumb.FirstOrDefault(c => c.EntityId == category.Id) != null
                select mcategory;
            var containingCategoryBreadcrumbMixCategories = containingCategoryBreadcrumbMixCategoriesQuery.ToList();
            foreach (var containingMixCategory in containingCategoryBreadcrumbMixCategories)
            {
                var containingCategory = await _categoryService.GetCategoryByIdAsync(containingMixCategory.EntityId);
                if (containingCategory == null)
                    return;
                containingMixCategory.Breadcrumb = await GetMixCategoryBreadcrumb(containingCategory)
                    .ConfigureAwait(false);
                await _mixCategoryService.UpdateMixCategoryAsync(containingMixCategory);
            }
        }
        #endregion
    }
}
