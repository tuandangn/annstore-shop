using Annstore.Core.Entities.Catalog;
using System;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.DataMixtures
{
    public sealed class CategoryDataMixturer : ICategoryDataMixturer
    {
        #region Fields
        private readonly ICategoryDataDependencyResolver _categoryDataDependencyResolver;
        #endregion

        #region Ctor
        public CategoryDataMixturer(ICategoryDataDependencyResolver categoryDataDependencyResolver)
        {
            _categoryDataDependencyResolver = categoryDataDependencyResolver;
        }
        #endregion

        #region Category
        public async Task<MixCategory> ApplyForCreatedCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (category.Deleted || !category.Published)
                return null;

            var mixCategory = await _categoryDataDependencyResolver.CreateMixCategoryForCategoryAsync(category)
                .ConfigureAwait(false);
            return mixCategory;
        }

        public async Task ApplyForDeletedCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await _categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(category)
                .ConfigureAwait(false);

            await _categoryDataDependencyResolver.UpdateBreadcrumbsOfCategoryAsync(category)
                .ConfigureAwait(false);
        }

        public async Task<MixCategory> ApplyForUpdatedCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            MixCategory updatedMixCategory;
            if (category.Deleted || !category.Published)
            {
                await _categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(category)
                    .ConfigureAwait(false);
                updatedMixCategory = null;
            }
            else
            {
                updatedMixCategory = await _categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(category)
                    .ConfigureAwait(false);
            }
            await _categoryDataDependencyResolver.UpdateBreadcrumbsOfCategoryAsync(category)
                .ConfigureAwait(false);

            return updatedMixCategory;
        }

        #endregion

        #region MixCategory
        public async Task ApplyForCreatedMixCategoryAsync(MixCategory mixCategory)
        {
            if (mixCategory == null)
                throw new ArgumentNullException(nameof(mixCategory));

            await _categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory)
                .ConfigureAwait(false);
        }

        public async Task ApplyForDeletedMixCategoryAsync(MixCategory mixCategory)
        {
            if (mixCategory == null)
                throw new ArgumentNullException(nameof(mixCategory));

            await _categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory)
                .ConfigureAwait(false);
        }

        public async Task ApplyForUpdatedMixCategoryAsync(MixCategory mixCategory)
        {
            if (mixCategory == null)
                throw new ArgumentNullException(nameof(mixCategory));

            await _categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
