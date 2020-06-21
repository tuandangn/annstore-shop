using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.Services.Catalog;
using Annstore.Services.Catalog;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QueryCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Tests.Services
{
    public class MixCategoryServiceTests
    {
        #region CreateMixCategoryAsync
        [Fact]
        public async Task CreateMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMixRepository<QueryCategory>>(), Mock.Of<IMapper>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.CreateMixCategoryAsync(null));
        }

        [Fact]
        public async Task CreateMixCategoryAsync()
        {
            var category = Category.CreateWithId(1);
            category.ParentId = 2;
            var mappedQueryCategory = new QueryCategory();
            var parentCategory = Category.CreateWithId(2);
            parentCategory.Published = true;
            var parentQueryCategory = new QueryCategory();
            var childrenCategories = new List<Category> { category };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(childrenCategories)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(parentCategory);
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(mappedQueryCategory))
                .ReturnsAsync(mappedQueryCategory)
                .Verifiable();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(category.ParentId))
                .ReturnsAsync(parentQueryCategory);
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(parentQueryCategory))
                .ReturnsAsync(parentQueryCategory)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<QueryCategory>(category))
                .Returns(mappedQueryCategory);
            var mixCategoryService = new MixCategoryService(categoryServiceMock.Object, mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.CreateMixCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task CreateMixCategoryAsync_ParentIdIs0_SkipUpdateParentMixCategory()
        {
            var category = Category.CreateWithId(1);
            category.ParentId = 0;
            var mappedQueryCategory = new QueryCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(mappedQueryCategory))
                .ReturnsAsync(mappedQueryCategory);
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<QueryCategory>(category))
                .Returns(mappedQueryCategory);
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.CreateMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<QueryCategory>()), Times.Never);
        }

        [Fact]
        public async Task CreateMixCategoryAsync_ParentCategoryIsNull_SkipUpdateParentMixCategory()
        {
            var notFoundCategoryId = 3;
            var category = Category.CreateWithId(1);
            category.ParentId = notFoundCategoryId;
            var mappedQueryCategory = new QueryCategory();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(mappedQueryCategory))
                .ReturnsAsync(mappedQueryCategory);
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<QueryCategory>(category))
                .Returns(mappedQueryCategory);
            var mixCategoryService = new MixCategoryService(categoryServiceMock.Object, mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.CreateMixCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<QueryCategory>()), Times.Never);
        }

        [Fact]
        public async Task CreateMixCategoryAsync_ParentCategoryIsNotVisible_SkipGetChildrenCategories()
        {
            var notVisibleCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notVisibleCategoryId;
            var notVisibleCategory = Category.CreateWithId(notVisibleCategoryId);
            notVisibleCategory.IsDeleted(true);
            notVisibleCategory.Published = false;
            var mappedQueryCategory = new QueryCategory();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notVisibleCategoryId))
                .ReturnsAsync(notVisibleCategory)
                .Verifiable();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(mappedQueryCategory))
                .ReturnsAsync(mappedQueryCategory);
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<QueryCategory>(category))
                .Returns(mappedQueryCategory);
            var mixCategoryService = new MixCategoryService(categoryServiceMock.Object, mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.CreateMixCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<QueryCategory>()), Times.Never);
        }

        [Fact]
        public async Task CreateMixCategoryAsync_ParentMixCategoryIsNull_SkipGetChildrenCategories()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var mappedQueryCategory = new QueryCategory();
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(mappedQueryCategory))
                .ReturnsAsync(mappedQueryCategory);
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(parentCategoryId))
                .ReturnsAsync((QueryCategory)null)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<QueryCategory>(category))
                .Returns(mappedQueryCategory);
            var mixCategoryService = new MixCategoryService(categoryServiceStub.Object, mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.CreateMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<QueryCategory>()), Times.Never);
        }
        #endregion

        #region DeleteMixCategoryAsync
        [Fact]
        public async Task DeleteMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMixRepository<QueryCategory>>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.DeleteMixCategoryAsync(null));
        }

        [Fact]
        public async Task DeleteMixCategoryAsync_MixCategoryIsNull_SkipDeleteMixCategory()
        {
            var notFoundCategoryId = 1;
            var category = Category.CreateWithId(notFoundCategoryId);
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(notFoundCategoryId))
                .ReturnsAsync((QueryCategory)null)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), mixCategoryRepositoryMock.Object, Mock.Of<IMapper>());

            await mixCategoryService.DeleteMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
            mixCategoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<QueryCategory>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMixCategoryAsync_MixCategoryIsNotNull_DeleteMixCategory()
        {
            var categoryId = 1;
            var category = Category.CreateWithId(categoryId);
            var queryCategory = new QueryCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(categoryId))
                .ReturnsAsync(queryCategory);
            mixCategoryRepositoryMock.Setup(r => r.DeleteAsync(queryCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), mixCategoryRepositoryMock.Object, Mock.Of<IMapper>());

            await mixCategoryService.DeleteMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task DeleteMixCategoryAsync_DeleteSuccess_UpdateChildrenOfMixCategory()
        {
            var categoryId = 1;
            var parentCategoryId = 2;
            var category = Category.CreateWithId(categoryId);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.Published = true;
            parentCategory.IsDeleted(false);
            var queryCategory = new QueryCategory();
            var parentQueryCategory = new QueryCategory();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(new List<Category>())
                .Verifiable();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(categoryId))
                .ReturnsAsync(queryCategory);
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(parentCategoryId))
                .ReturnsAsync(parentQueryCategory)
                .Verifiable();
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(parentQueryCategory))
                .ReturnsAsync(parentQueryCategory)
                .Verifiable();
            mixCategoryRepositoryMock.Setup(r => r.DeleteAsync(queryCategory))
                .Returns(Task.CompletedTask);
            var mixCategoryService = new MixCategoryService(categoryServiceMock.Object, mixCategoryRepositoryMock.Object, Mock.Of<IMapper>());

            await mixCategoryService.DeleteMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
            categoryServiceMock.Verify();
        }

        #endregion

        #region UpdateMixCategoryAsync
        [Fact]
        public async Task UpdateMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMixRepository<QueryCategory>>(), Mock.Of<IMapper>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.UpdateMixCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateMixCategoryAsync_MixCategoryIsNull_SkipUpdateMixCategory()
        {
            var notFoundMixCategoryEntityId = 1;
            var category = Category.CreateWithId(notFoundMixCategoryEntityId);
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(notFoundMixCategoryEntityId))
                .ReturnsAsync((QueryCategory)null)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), mixCategoryRepositoryMock.Object, Mock.Of<IMapper>());

            await mixCategoryService.UpdateMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
            mixCategoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<QueryCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMixCategoryAsync_MixCategoryIsNotNull_UpdateMixCategory()
        {
            var categoryId = 1;
            var category = Category.CreateWithId(categoryId);
            var queryCategory = new QueryCategory();
            var updatedQueryCategory = new QueryCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(categoryId))
                .ReturnsAsync(queryCategory);
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(updatedQueryCategory))
                .ReturnsAsync(updatedQueryCategory)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Category, QueryCategory>(category, queryCategory))
                .Returns(updatedQueryCategory)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(Mock.Of<ICategoryService>(), mixCategoryRepositoryMock.Object, mapperMock.Object);

            await mixCategoryService.UpdateMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
        }

        [Fact]
        public async Task UpdateMixCategoryAsync_UpdateChildrenOfMixParentCategory()
        {
            var categoryId = 1;
            var parentCategoryId = 2;
            var category = Category.CreateWithId(categoryId);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var queryCategory = new QueryCategory();
            var parentQueryCategory = new QueryCategory();
            var updatedQueryCategory = new QueryCategory();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(new List<Category>())
                .Verifiable();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<QueryCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(categoryId))
                .ReturnsAsync(queryCategory);
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(updatedQueryCategory))
                .ReturnsAsync(updatedQueryCategory);
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(parentCategoryId))
                .ReturnsAsync(parentQueryCategory)
                .Verifiable();
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(parentQueryCategory))
                .ReturnsAsync(parentQueryCategory)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<Category, QueryCategory>(category, queryCategory))
                .Returns(updatedQueryCategory);
            var mixCategoryService = new MixCategoryService(categoryServiceMock.Object, mixCategoryRepositoryMock.Object, mapperStub.Object);

            await mixCategoryService.UpdateMixCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryRepositoryMock.Verify();
        }

        #endregion
    }
}
