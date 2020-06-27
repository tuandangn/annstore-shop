using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.DataMixtures;
using AutoMapper;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Tests.DataMixtures
{
    public class CategoryDataMixturerTests
    {
        #region ApplyForCreatedCategoryAsync
        [Fact]
        public async Task ApplyForCreatedCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForCreatedCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForCreatedCategoryAsync_CategoryIsVisible_CreateMixCategoryAndUpdateChildrenOfParentMixCategory()
        {
            var createdCategory = Category.CreateWithId(1);
            createdCategory.Published = true;
            createdCategory.IsDeleted(false);
            var expectedResult = new MixCategory();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(m => m.CreateMixCategoryForCategoryAsync(createdCategory))
                .ReturnsAsync(expectedResult)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            var result = await categoryDataMixturer.ApplyForCreatedCategoryAsync(createdCategory);

            Assert.Equal(expectedResult, result);
            categoryDataDependencyResolverMock.Verify();
        }

        [Fact]
        public async Task ApplyForCreatedCategoryAsync_CategoryIsNotVisible_Skip()
        {
            var createdCategory = new Category();
            createdCategory.IsDeleted(true);
            createdCategory.Published = false;
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            var result = await categoryDataMixturer.ApplyForCreatedCategoryAsync(createdCategory);

            Assert.Null(result);
            categoryDataDependencyResolverMock.Verify(m => m.CreateMixCategoryForCategoryAsync(It.IsAny<Category>()), Times.Never);
        }
        #endregion

        #region ApplyForDeletedCategoryAsync
        [Fact]
        public async Task ApplyForDeletedCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForDeletedCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForDeletedCategoryAsync_CategoryIsNotNull_DeleteMixCategoryAndUpdateChildrenOfParentMixCategory()
        {
            var deletedCategory = new Category();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.DeleteMixCategoryForCategoryAsync(deletedCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateBreadcrumbsOfCategoryAsync(deletedCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            await categoryDataMixturer.ApplyForDeletedCategoryAsync(deletedCategory);

            categoryDataDependencyResolverMock.Verify();
        }

        #endregion

        #region ApplyForUpdatedCategoryAsync
        [Fact]
        public async Task ApplyForUpdatedCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForUpdatedCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForUpdatedCategoryAsync_CategoryIsVisible_UpdateMixCategoryAndUpdateChildrenOfParentMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            updatedCategory.IsDeleted(false);
            updatedCategory.Published = true;
            var expectedResult = new MixCategory();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateMixCategoryForCategoryAsync(updatedCategory))
                .ReturnsAsync(expectedResult)
                .Verifiable();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateBreadcrumbsOfCategoryAsync(updatedCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            var result = await categoryDataMixturer.ApplyForUpdatedCategoryAsync(updatedCategory);

            Assert.Equal(expectedResult, result);
            categoryDataDependencyResolverMock.Verify();
        }


        [Fact]
        public async Task ApplyForUpdatedCategoryAsync_CategoryIsNotVisible_DeleteMixCategoryAndUpdateChildrenOfParentMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            updatedCategory.IsDeleted(true);
            updatedCategory.Published = false;
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.DeleteMixCategoryForCategoryAsync(updatedCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            var result = await categoryDataMixturer.ApplyForUpdatedCategoryAsync(updatedCategory);

            Assert.Null(result);
            categoryDataDependencyResolverMock.Verify();
        }

        #endregion

        #region ApplyForCreatedMixCategoryAsync
        [Fact]
        public async Task ApplyForCreatedMixCategoryAsync_MixCategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForCreatedMixCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForCreatedMixCategoryAsync_MixCategoryIsNotNull_UpdateChildrenOfParentMixCategoryForIt()
        {
            var createdMixCategory = new MixCategory();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateChildrenOfMixParentCategoryForAsync(createdMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            await categoryDataMixturer.ApplyForCreatedMixCategoryAsync(createdMixCategory);

            categoryDataDependencyResolverMock.Verify();
        }

        #endregion

        #region ApplyForDeletedMixCategoryAsync
        [Fact]
        public async Task ApplyForDeletedMixCategoryAsync_MixCategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForDeletedMixCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForDeletedMixCategoryAsync_MixCategoryIsNotNull_UpdateChildrenOfParentMixCategoryForItAndUpdateContainingCategoryBreacrumbs()
        {
            var deletedMixCategory = new MixCategory();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateChildrenOfMixParentCategoryForAsync(deletedMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            await categoryDataMixturer.ApplyForDeletedMixCategoryAsync(deletedMixCategory);

            categoryDataDependencyResolverMock.Verify();
        }

        #endregion

        #region ApplyForUpdatedMixCategoryAsync
        [Fact]
        public async Task ApplyForUpdatedMixCategoryAsync_MixCategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataMixturer = new CategoryDataMixturer(Mock.Of<ICategoryDataDependencyResolver>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => categoryDataMixturer.ApplyForUpdatedMixCategoryAsync(null));
        }

        [Fact]
        public async Task ApplyForUpdatedMixCategoryAsync_MixCategoryIsNotNull_UpdateChildrenOfParentMixCategoryForItAndUpdateContainingCategoryBreadcrumbs()
        {
            var updatedMixCategory = new MixCategory();
            var categoryDataDependencyResolverMock = new Mock<ICategoryDataDependencyResolver>();
            categoryDataDependencyResolverMock.Setup(c => c.UpdateChildrenOfMixParentCategoryForAsync(updatedMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataMixturer = new CategoryDataMixturer(categoryDataDependencyResolverMock.Object);

            await categoryDataMixturer.ApplyForUpdatedMixCategoryAsync(updatedMixCategory);

            categoryDataDependencyResolverMock.Verify();
        }

        #endregion
    }
}
