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
            var availableCategories = new[] { new Category { Id = 1 }, new Category { Id = 2 } };
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock
                .Setup(r => r.Table).Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetCategoriesAsync();

            Assert.Equal(availableCategories.Length, result.Count);
            categoryRepositoryMock.Verify();
        }
        #endregion

        #region GetCategoryByIdAsync

        [Fact]
        public async Task GetCategoryByIdAsync_FindCategoryById()
        {
            var id = 1;
            var category = new Category { Id = id };
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
            categoryRepositoryMock.Setup(r => r.DeleteAsync(category))
                .Returns(Task.CompletedTask)
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

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryService.GetCategoryBreadcrumbAsync(null, 0));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsLessThanZeror_ThrowArgumentException()
        {
            var deepLevel = -1;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetCategoryBreadcrumbAsync(new Category(), deepLevel));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsZeror_ReturnContainedCategoryList()
        {
            var category = new Category();
            var deepLevel = 0;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel);

            Assert.Single(categoryBreadcrumb);
            Assert.Equal(category, categoryBreadcrumb[0]);
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_ParentIdIsZero_ReturnContainedCategoryList()
        {
            var category = new Category { ParentId = 0 };
            var deepLevel = 1;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel);

            Assert.Single(categoryBreadcrumb);
            Assert.Equal(category, categoryBreadcrumb[0]);
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_DeepLevelIsGreaterThanZero_IncludeParentCategoryHierarchy()
        {
            var category = new Category { Id = 1, ParentId = 2 };
            var parentCategory = new Category { Id = 2, ParentId = 3 };
            var ancestorCategory = new Category { Id = 3 };
            var deepLevel = 2;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(2))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(3))
                .ReturnsAsync(ancestorCategory)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel);

            Assert.Equal(3, categoryBreadcrumb.Count);
            Assert.Equal(ancestorCategory, categoryBreadcrumb[0]);
            Assert.Equal(parentCategory, categoryBreadcrumb[1]);
            Assert.Equal(category, categoryBreadcrumb[2]);
            categoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbAsync_ParentCategoryIsNotFound_ReturnCurrentResult()
        {
            var notFoundCategoryId = 5;
            var category = new Category { Id = 1, ParentId = 2 };
            var parentCategory = new Category { Id = 2, ParentId = notFoundCategoryId };
            var deepLevel = 2;
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(2))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryRepositoryMock.Setup(c => c.FindByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var categoryBreadcrumb = await categoryService.GetCategoryBreadcrumbAsync(category, deepLevel);

            Assert.Equal(2, categoryBreadcrumb.Count);
            Assert.Equal(parentCategory, categoryBreadcrumb[0]);
            Assert.Equal(category, categoryBreadcrumb[1]);
            categoryRepositoryMock.Verify();
        }

        #endregion

        #region GetPagedCategoriesAsync
        [Fact]
        public async Task GetPagedCategoriesAsync_PageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetPagedCategoriesAsync(pageNumber, int.MaxValue));
        }

        [Fact]
        public async Task GetPagedCategoriesAsync_PageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var categoryService = new CategoryService(Mock.Of<IRepository<Category>>());

            await Assert.ThrowsAsync<ArgumentException>(() => categoryService.GetPagedCategoriesAsync(1, pageSize));
        }

        [Fact]
        public async Task GetPagedCategoriesAsync_ReturnValidResult()
        {
            var pageNumber = 2;
            var pageSize = 2;
            var availableCategories = new List<Category>
            {
                new Category{Id = 1 }, new Category{Id = 2 },
                new Category{Id = 3 }, new Category{Id = 4 },
                new Category{Id = 5 }, new Category{Id = 6 }
            };
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock.Setup(c => c.Table)
                .Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);

            var result = await categoryService.GetPagedCategoriesAsync(pageNumber, pageSize);

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(availableCategories.Count, result.TotalItems);
            Assert.Equal(3, result.Items.ElementAt(0).Id);
            categoryRepositoryMock.Verify();
        }

        #endregion
    }
}
