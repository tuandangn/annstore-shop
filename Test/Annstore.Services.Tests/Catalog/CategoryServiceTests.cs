using System;
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
            var category = new Category{Id = id};
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
    }
}
