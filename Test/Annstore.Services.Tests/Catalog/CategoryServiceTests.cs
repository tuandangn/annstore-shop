using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;
using Annstore.Data;
using Annstore.Services.Catalog;
using Moq;
using TestHelper;
using Xunit;

namespace Annstore.Services.Tests.Catalog
{
    public class CategoryServiceTests
    {
        #region GetCategoriesAsync
        [Fact]
        public async Task GetCategoriesAsync_ReturnAllCategories()
        {
            var publishedCategory = Category.CreateWithId(1);
            publishedCategory.Published = true;
            var unpublishedCategory = Category.CreateWithId(2);
            unpublishedCategory.Published = false;

            var availableCategories = new[] { publishedCategory, unpublishedCategory };
            var showHidden = false;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock
                .Setup(r => r.Table).Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetCategoriesAsync(showHidden);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetCategoriesAsync_ShowHidden_IncludeNotPublishedCategories()
        {
            var unpublishedCategory1 = Category.CreateWithId(1);
            var unpublishedCategory2 = Category.CreateWithId(2);
            var availableCategories = new[] { unpublishedCategory1, unpublishedCategory2 };
            var showHidden = true;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock
                .Setup(r => r.Table).Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetCategoriesAsync(showHidden);

            Assert.Equal(availableCategories.Length, result.Count);
            categoryRepositoryMock.Verify();
        }
        #endregion

        #region GetCategoryByIdAsync

        [Fact]
        public async Task GetCategoryByIdAsync_FindCategoryById()
        {
            var id = 1;
            var category = Category.CreateWithId(id);
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(r => r.FindByIdAsync(id))
                .ReturnsAsync(category)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetCategoryByIdAsync(id);

            Assert.Equal(category, result);
            categoryRepositoryMock.Verify();
        }
        #endregion

        #region UpdateCategoryAsync
        [Fact]
        public async Task UpdateCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryService.UpdateCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryIsNotNull_UpdateCategory()
        {
            var category = new Category();
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(r => r.UpdateAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.UpdateCategoryAsync(category);

            Assert.Equal(category, result);
            categoryRepositoryMock.Verify();
        }
        #endregion

        #region CreateCategoryAsync

        [Fact]
        public async Task CreateCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryService.CreateCategoryAsync(null));
        }

        [Fact]
        public async Task CreateCategoryAsync_CategoryIsNotNull_InsertCategory()
        {
            var category = new Category();
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(r => r.InsertAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.CreateCategoryAsync(category);

            Assert.Equal(category, result);
            categoryRepositoryMock.Verify();
        }
        #endregion

        #region DeleteCategoryAsync

        [Fact]
        public async Task DeleteCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryService.DeleteCategoryAsync(null));
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryIsNotNull_DeleteCategory()
        {
            var category = new Category();
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Category>(cat => cat.Deleted)))
                .ReturnsAsync(category)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            await categoryService.DeleteCategoryAsync(category);

            categoryRepositoryMock.Verify();
        }

        #endregion

        #region GetCategoryBreadcrumbAsync
        [Fact]
        public async Task GetCategoryBreadcrumbAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryService.GetCategoryBreadcrumbAsync(null, 0, true));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsLessThanZeror_ThrowArgumentException()
        {
            var deepLevel = -1;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetCategoryBreadcrumbAsync(new Category(), deepLevel, true));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsZeror_ReturnContainedCategoryList()
        {
            var category = new Category();
            var deepLevel = 0;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel, true);

            Assert.Single(categoryBreadcrumb);
            Assert.Equal(category, categoryBreadcrumb[0]);
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_ParentIdIsZero_ReturnContainedCategoryList()
        {
            var category = new Category { ParentId = 0 };
            var deepLevel = 1;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel, true);

            Assert.Single(categoryBreadcrumb);
            Assert.Equal(category, categoryBreadcrumb[0]);
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsGreaterThanZero_IncludeParentCategoryHierarchy()
        {
            var category1WithParentIdIs2 = Category.CreateWithId(1);
            category1WithParentIdIs2.ParentId = 2;
            var category2WithParentIdIs3 = Category.CreateWithId(2);
            category2WithParentIdIs3.ParentId = 3;
            var category3WithParentIdIs0 = Category.CreateWithId(3);
            var deepLevel = 2;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            var showHidden = true;
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(2))
                .ReturnsAsync(category2WithParentIdIs3)
                .Verifiable();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(3))
                .ReturnsAsync(category3WithParentIdIs0)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category1WithParentIdIs2, deepLevel, showHidden);

            Assert.Equal(3, categoryBreadcrumb.Count);
            Assert.Equal(category3WithParentIdIs0, categoryBreadcrumb[0]);
            Assert.Equal(category2WithParentIdIs3, categoryBreadcrumb[1]);
            Assert.Equal(category1WithParentIdIs2, categoryBreadcrumb[2]);
            categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_ShowHiddenIsFalse_ExcludeNotPublishedAndDeletedCategories()
        {
            var category1WithParentIdIs2 = Category.CreateWithId(1);
            category1WithParentIdIs2.ParentId = 2;
            var deletedCategory2WithParentIdIs3 = Category.CreateWithId(2);
            deletedCategory2WithParentIdIs3.IsDeleted(true);
            deletedCategory2WithParentIdIs3.ParentId = 3;
            var unpublishedCategory3WithParentIdIs0 = Category.CreateWithId(3);
            unpublishedCategory3WithParentIdIs0.Published = false;
            var deepLevel = 2;
            var showHidden = false;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(2))
                .ReturnsAsync(deletedCategory2WithParentIdIs3)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category1WithParentIdIs2, deepLevel, showHidden);

            Assert.Single(categoryBreadcrumb);
            Assert.Equal(category1WithParentIdIs2, categoryBreadcrumb[0]);
            categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_ParentCategoryIsNotFound_ReturnCurrentResult()
        {
            var notFoundCategoryId = 5;
            var category1WithParentIdIs2 = Category.CreateWithId(1);
            category1WithParentIdIs2.ParentId = 2;
            var category2WithParentIdIsNotFound = Category.CreateWithId(2);
            category2WithParentIdIsNotFound.ParentId = notFoundCategoryId;
            var deepLevel = 2;
            var showHidden = true;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(2))
                .ReturnsAsync(category2WithParentIdIsNotFound)
                .Verifiable();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category1WithParentIdIs2, deepLevel, showHidden);

            Assert.Equal(2, categoryBreadcrumb.Count);
            Assert.Equal(category2WithParentIdIsNotFound, categoryBreadcrumb[0]);
            Assert.Equal(category1WithParentIdIs2, categoryBreadcrumb[1]);
            categoryRepositoryMock.Verify();
        }

        #endregion

        #region GetPagedCategoriesAsync
        [Fact]
        public async Task GetPagedCategoriesAsync_PageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var showHidden = true;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetPagedCategoriesAsync(pageNumber, int.MaxValue, showHidden));
        }

        [Fact]
        public async Task GetPagedCategoriesAsync_PageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var showHidden = true;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetPagedCategoriesAsync(1, pageSize, showHidden));
        }

        [Fact]
        public async Task GetPagedCategoriesAsync_ReturnValidResult()
        {
            var pageNumber = 2;
            var pageSize = 2;
            var showHidden = true;
            var availableCategories = new List<Category>
            {
                Category.CreateWithId(1), Category.CreateWithId(2),
                Category.CreateWithId(3), Category.CreateWithId(4),
                Category.CreateWithId(5), Category.CreateWithId(6)
            };
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.Table)
                .Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetPagedCategoriesAsync(pageNumber, pageSize, showHidden);

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(availableCategories.Count, result.TotalItems);
            //order by id desc
            Assert.Equal(4, result.Items.ElementAt(0).Id);
            categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetPagedCategoriesAsync_ShowHiddenIsFalse_ExcludeNotPublishedAndDeletedCategories()
        {
            var publishedCategory1 = Category.CreateWithId(1);
            publishedCategory1.Published = true;
            var publishedCategory2 = Category.CreateWithId(2);
            publishedCategory2.Published = true;
            var publishedCategory3 = Category.CreateWithId(3);
            publishedCategory3.Published = true;
            var unpublishedCategory1 = Category.CreateWithId(4);
            unpublishedCategory1.Published = false;
            var unpublishedCategory2 = Category.CreateWithId(5);
            unpublishedCategory2.Published = false;
            var unpublishedCategory3 = Category.CreateWithId(6);
            unpublishedCategory3.Published = false;
            var pageNumber = 2;
            var pageSize = 2;
            var showHidden = false;
            var availableCategories = new List<Category>
            {
                publishedCategory1, publishedCategory2, publishedCategory3,
                unpublishedCategory1, unpublishedCategory2, unpublishedCategory3
            };
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.Table)
                .Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetPagedCategoriesAsync(pageNumber, pageSize, showHidden);

            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.TotalItems);
            //order by id desc
            Assert.Equal(1, result.Items.ElementAt(0).Id);
            categoryRepositoryMock.Verify();
        }

        #endregion
    }
}
