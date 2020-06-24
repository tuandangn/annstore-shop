using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.DataMixtures;
using Annstore.DataMixture.Services.Catalog;
using Annstore.Services.Catalog;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Tests.DataMixtures
{
    public class CategoryDataDependencyResolverTests
    {
        #region CreateMixCategoryForCategoryAsync
        [Fact]
        public async Task CreateMixCategoryForCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                Mock.Of<IMixCategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.CreateMixCategoryForCategoryAsync(null));
        }

        [Fact]
        public async Task CreateMixCategoryForCategoryAsync_CategoryIsNotNull_CreateMixCategory()
        {
            var createdCategory = Category.CreateWithId(1);
            createdCategory.Published = true;
            createdCategory.IsDeleted(false);
            var mappedMixCategory = new MixCategory();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<MixCategory>(createdCategory))
                .Returns(mappedMixCategory)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.CreateMixCategoryAsync(mappedMixCategory))
                .ReturnsAsync(mappedMixCategory)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, mapperMock.Object);

            var result = await categoryDataDependencyResolver.CreateMixCategoryForCategoryAsync(createdCategory);

            Assert.Equal(mappedMixCategory, result);
            mixCategoryServiceMock.Verify();
            mapperMock.Verify();
        }
        #endregion

        #region UpdateChildrenOfMixParentCategoryAsync
        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                Mock.Of<IMixCategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(null));

        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_ParentCategoryIsZero_Skip()
        {
            var category = Category.CreateWithId(1);
            category.ParentId = 0;
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, Mock.Of<IMapper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(category);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_ParentCategoryIsNotFound_Skip()
        {
            var notFoundCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notFoundCategoryId;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(categoryServiceMock.Object,
                mixCategoryServiceMock.Object, Mock.Of<IMapper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_ParentCategoryIsNotVisible_Skip()
        {
            var notVisibleCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notVisibleCategoryId;
            var parentCategory = Category.CreateWithId(notVisibleCategoryId);
            parentCategory.IsDeleted(true);
            parentCategory.Published = false;
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(notVisibleCategoryId))
                .ReturnsAsync(parentCategory);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(categoryServiceStub.Object,
                mixCategoryServiceMock.Object, Mock.Of<IMapper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(category);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_ParentMixCategoryIsNotFound_InsertParentMixCategory()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceStub.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(new List<Category>());
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<MixCategory>(parentCategory))
                .Returns(mappedParentMixCategory);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(parentCategoryId))
                .ReturnsAsync((MixCategory)null)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.CreateMixCategoryAsync(mappedParentMixCategory))
                .ReturnsAsync(mappedParentMixCategory)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()))
                .ReturnsAsync((MixCategory)null);
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(categoryServiceStub.Object,
                mixCategoryServiceMock.Object, mapperStub.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(category);

            mixCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryAsync_ParentMixCategoryIsFound_UpdateChildren()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var parentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var childCategory = Category.CreateWithId(3);
            var chilrenOfParentCategory = new List<Category> { childCategory };
            var mappedChildCategory = new MixCategory { EntityId = childCategory.Id };
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(chilrenOfParentCategory)
                .Verifiable(); ;
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<MixCategory>(childCategory))
                .Returns(mappedChildCategory);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(parentCategoryId))
                .ReturnsAsync(parentMixCategory)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.UpdateMixCategoryAsync(parentMixCategory))
                .ReturnsAsync(parentMixCategory)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(categoryServiceMock.Object,
                mixCategoryServiceMock.Object, mapperStub.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryAsync(category);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(
                It.Is<MixCategory>(mc => mc.Children.FirstOrDefault(c => c.EntityId == childCategory.Id) != null)));
        }
        #endregion

        #region DeleteMixCategoryForCategoryAsync
        [Fact]
        public async Task DeleteMixCategoryForCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                Mock.Of<IMixCategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(null));
        }

        [Fact]
        public async Task DeleteMixCategoryForCategoryAsync_MixCategoryIsNotFound_Skip()
        {
            var deletedCategoryId = 1;
            var deletedCategory = Category.CreateWithId(deletedCategoryId);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(deletedCategoryId))
                .ReturnsAsync((MixCategory)null)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, Mock.Of<IMapper>());

            await categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(deletedCategory);

            mixCategoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.DeleteMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMixCategoryForCategoryAsync_MixCategoryIsFound_DeleteMixCategory()
        {
            var deletedCategoryId = 1;
            var category = Category.CreateWithId(deletedCategoryId);
            var deletedMixCategory = new MixCategory { EntityId = deletedCategoryId };
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(deletedCategoryId))
                .ReturnsAsync(deletedMixCategory);
            mixCategoryServiceMock.Setup(m => m.DeleteMixCategoryAsync(deletedMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, Mock.Of<IMapper>());

            await categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(category);

            mixCategoryServiceMock.Verify();
        }

        #endregion

        #region UpdateMixCategoryForCategoryAsync
        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                Mock.Of<IMixCategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_MixCategoryIsNotNull_UpdateMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            var mixCategory = new MixCategory { EntityId = updatedCategory.Id };
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(updatedCategory.Id))
                .ReturnsAsync(mixCategory)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.UpdateMixCategoryAsync(mixCategory))
                .ReturnsAsync(mixCategory)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Category, MixCategory>(updatedCategory, mixCategory))
                .Returns(mixCategory)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, mapperMock.Object);

            var result = await categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(updatedCategory);

            Assert.Equal(mixCategory, result);
            mapperMock.Verify();
            mixCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_MixCategoryIsNull_InsertMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            var insertedMixCategory = new MixCategory();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(updatedCategory.Id))
                .ReturnsAsync((MixCategory) null)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.CreateMixCategoryAsync(insertedMixCategory))
                .ReturnsAsync(insertedMixCategory)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<MixCategory>(updatedCategory))
                .Returns(insertedMixCategory);
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(Mock.Of<ICategoryService>(),
                mixCategoryServiceMock.Object, mapperStub.Object);

            var result = await categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(updatedCategory);

            Assert.Equal(insertedMixCategory, result);
            mixCategoryServiceMock.Verify();
        }
        #endregion
    }
}
