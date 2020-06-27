using Annstore.Core.Common;
using Annstore.Core.Entities.Catalog;
using Annstore.DataMixture.DataMixtures;
using Annstore.DataMixture.Services.Catalog;
using Annstore.Query.Infrastructure;
using Annstore.Services.Catalog;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestHelper;
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
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.CreateMixCategoryForCategoryAsync(null));
        }

        [Fact]
        public async Task CreateMixCategoryForCategoryAsync_CategoryIsNotNull_CreateMixCategory()
        {
            var createdCategory = Category.CreateWithId(1);
            createdCategory.Published = true;
            createdCategory.IsDeleted(false);
            createdCategory.Name = "created category";
            var firstBreadcrumbCategory = Category.CreateWithId(2);
            var mappedFirstBreadcrumbMixCategory = new MixCategory { EntityId = 2 };
            var categoryBreadcrumb = new List<Category> { firstBreadcrumbCategory, createdCategory };
            var mappedMixCategory = new MixCategory();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<MixCategory>(createdCategory))
                .Returns(mappedMixCategory)
                .Verifiable();
            mapperMock.Setup(m => m.Map<MixCategory>(firstBreadcrumbCategory))
                .Returns(mappedFirstBreadcrumbMixCategory)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.CreateMixCategoryAsync(mappedMixCategory))
                .ReturnsAsync(mappedMixCategory)
                .Verifiable();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(createdCategory, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false))
                .ReturnsAsync(categoryBreadcrumb)
                .Verifiable();
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(createdCategory.Name))
                .Returns(createdCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                mapperMock.Object, stringHelperMock.Object);

            var result = await categoryDataDependencyResolver.CreateMixCategoryForCategoryAsync(createdCategory);

            Assert.Equal(mappedMixCategory, result);
            Assert.Equal(2, mappedMixCategory.Breadcrumb.Count());
            Assert.Equal(mappedFirstBreadcrumbMixCategory, mappedMixCategory.Breadcrumb.ElementAt(0));
            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify();
            mapperMock.Verify();
            stringHelperMock.Verify();
        }
        #endregion

        #region UpdateChildrenOfMixParentCategoryOfAsync
        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(null));

        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_ParentCategoryIsZero_Skip()
        {
            var category = Category.CreateWithId(1);
            category.ParentId = 0;
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(category);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_ParentCategoryIsNotFound_Skip()
        {
            var notFoundCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notFoundCategoryId;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(category);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_ParentCategoryIsNotVisible_Skip()
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
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceStub.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(category);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_ParentMixCategoryIsNotFound_InsertParentMixCategory()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.Name = "category";
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            parentCategory.Name = "parent category";
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceStub.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(new List<Category>());
            categoryServiceStub.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false))
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
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(parentCategory.Name))
                .Returns(parentCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceStub.Object, mixCategoryServiceMock.Object,
                mapperStub.Object, stringHelperMock.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(category);

            mixCategoryServiceMock.Verify();
            stringHelperMock.Verify();
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryOfAsync_ParentMixCategoryIsFound_UpdateChildren()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var parentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var childCategory = Category.CreateWithId(3);
            childCategory.Name = "child";
            var chilrenOfParentCategory = new List<Category> { childCategory };
            var mappedChildCategory = new MixCategory { EntityId = childCategory.Id };
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(chilrenOfParentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(childCategory))
                .ReturnsAsync(new List<Category>())
                .Verifiable();
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
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(childCategory.Name))
                .Returns(childCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                mapperStub.Object, stringHelperMock.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryOfAsync(category);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(
                It.Is<MixCategory>(mc => mc.Children.FirstOrDefault(c => c.EntityId == childCategory.Id) != null)));
            stringHelperMock.Verify();
        }
        #endregion

        #region UpdateChildrenOfMixParentCategoryForAsync
        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_MixCategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(null));

        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_CategoryIsNotFound_Skip()
        {
            var notFoundCategoryId = 1;
            var mixCategory = new MixCategory { EntityId = notFoundCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_ParentCategoryIsZero_Skip()
        {
            var parentIsZerorCategoryId = 1;
            var category = Category.CreateWithId(parentIsZerorCategoryId);
            category.ParentId = 0;
            var mixCategory = new MixCategory { EntityId = parentIsZerorCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentIsZerorCategoryId))
                .ReturnsAsync(category);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_ParentCategoryIsNotFound_Skip()
        {
            var notFoundCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notFoundCategoryId;
            var mixCategory = new MixCategory { EntityId = category.Id };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_ParentCategoryIsNotVisible_Skip()
        {
            var notVisibleCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = notVisibleCategoryId;
            var parentCategory = Category.CreateWithId(notVisibleCategoryId);
            parentCategory.IsDeleted(true);
            parentCategory.Published = false;
            var mixCategory = new MixCategory { EntityId = category.Id };
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(category.Id))
                .ReturnsAsync(category);
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(notVisibleCategoryId))
                .ReturnsAsync(parentCategory);
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceStub.Object, mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(It.IsAny<MixCategory>()), Times.Never);
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_ParentMixCategoryIsNotFound_InsertParentMixCategory()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            parentCategory.Name = "parent";
            var mixCategory = new MixCategory { EntityId = category.Id };
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(category.Id))
                .ReturnsAsync(category);
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceStub.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(new List<Category>());
            categoryServiceStub.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false))
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
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(parentCategory.Name))
                .Returns(parentCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceStub.Object, mixCategoryServiceMock.Object,
                mapperStub.Object, stringHelperMock.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            mixCategoryServiceMock.Verify();
            stringHelperMock.Verify();
        }

        [Fact]
        public async Task UpdateChildrenOfMixParentCategoryForAsync_ParentMixCategoryIsFound_UpdateChildren()
        {
            var parentCategoryId = 2;
            var category = Category.CreateWithId(1);
            category.ParentId = parentCategoryId;
            var parentCategory = Category.CreateWithId(parentCategoryId);
            parentCategory.IsDeleted(false);
            parentCategory.Published = true;
            var mixCategory = new MixCategory { EntityId = category.Id };
            var parentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var childCategory = Category.CreateWithId(3);
            childCategory.Name = "child";
            var chilrenOfParentCategory = new List<Category> { childCategory };
            var mappedChildCategory = new MixCategory { EntityId = childCategory.Id };
            var mappedParentMixCategory = new MixCategory { EntityId = parentCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(parentCategoryId))
                .ReturnsAsync(parentCategory);
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(parentCategory))
                .ReturnsAsync(chilrenOfParentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetChildrenCategoriesAsync(childCategory))
                .ReturnsAsync(new List<Category>())
                .Verifiable();
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
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(childCategory.Name))
                .Returns(childCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                mapperStub.Object, stringHelperMock.Object);

            await categoryDataDependencyResolver.UpdateChildrenOfMixParentCategoryForAsync(mixCategory);

            categoryServiceMock.Verify();
            mixCategoryServiceMock.Verify();
            mixCategoryServiceMock.Verify(m => m.UpdateMixCategoryAsync(
                It.Is<MixCategory>(mc => mc.Children.FirstOrDefault(c => c.EntityId == childCategory.Id) != null)));
            stringHelperMock.Verify();
        }
        #endregion

        #region DeleteMixCategoryForCategoryAsync
        [Fact]
        public async Task DeleteMixCategoryForCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

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
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

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
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), mixCategoryServiceMock.Object,
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await categoryDataDependencyResolver.DeleteMixCategoryForCategoryAsync(category);

            mixCategoryServiceMock.Verify();
        }

        #endregion

        #region UpdateMixCategoryForCategoryAsync
        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_MixCategoryIsNotNull_UpdateMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            updatedCategory.Name = "updated category";
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
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(updatedCategory.Name))
                .Returns(updatedCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), mixCategoryServiceMock.Object,
                mapperMock.Object, stringHelperMock.Object);

            var result = await categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(updatedCategory);

            Assert.Equal(mixCategory, result);
            mapperMock.Verify();
            mixCategoryServiceMock.Verify();
            stringHelperMock.Verify();
        }

        [Fact]
        public async Task UpdateMixCategoryForCategoryAsync_MixCategoryIsNull_InsertMixCategory()
        {
            var updatedCategory = Category.CreateWithId(1);
            updatedCategory.Name = "updated category";
            var insertedMixCategory = new MixCategory();
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryBreadcrumbAsync(updatedCategory, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false))
                .ReturnsAsync(new List<Category>());
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetMixCategoryByEntityIdAsync(updatedCategory.Id))
                .ReturnsAsync((MixCategory)null)
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.CreateMixCategoryAsync(insertedMixCategory))
                .ReturnsAsync(insertedMixCategory)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<MixCategory>(updatedCategory))
                .Returns(insertedMixCategory);
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(updatedCategory.Name))
                .Returns(updatedCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceStub.Object, mixCategoryServiceMock.Object,
                mapperStub.Object, stringHelperMock.Object);

            var result = await categoryDataDependencyResolver.UpdateMixCategoryForCategoryAsync(updatedCategory);

            Assert.Equal(insertedMixCategory, result);
            mixCategoryServiceMock.Verify();
            stringHelperMock.Verify();
        }
        #endregion

        #region UpdateBreadcrumbsOfCategoryAsync
        [Fact]
        public async Task UpdateBreadcrumbsOfMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                Mock.Of<ICategoryService>(), Mock.Of<IMixCategoryService>(),
                Mock.Of<IMapper>(), Mock.Of<IStringHelper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                categoryDataDependencyResolver.UpdateBreadcrumbsOfCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateBreadcrumbsOfMixCategoryAsync_CategoryIsNotNull_UpdateBreadcrumbsOfCategory()
        {
            var categoryId = 1;
            var category = Category.CreateWithId(categoryId);
            category.IsDeleted(true);
            category.Published = false;
            var containingCategoryId = 2;
            var containingCategory = Category.CreateWithId(containingCategoryId);
            containingCategory.Name = "containing category";
            var mappedContainingCategory = new MixCategory();
            var containingCategoryBreadcrumb = new List<Category> { containingCategory };
            var targetItem = new MixCategory { EntityId = categoryId };
            var containingCategoryBreadcrumbMixCategory = new MixCategory
            {
                EntityId = containingCategoryId,
                Breadcrumb = new List<MixCategory>
                {
                    targetItem
                }
            };
            var availableMixCategories = new List<MixCategory> { containingCategoryBreadcrumbMixCategory };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(containingCategoryId))
                .ReturnsAsync(containingCategory);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(containingCategory, QueryCategorySettings.Breadcrumb.DEEP_LEVEL, false))
                .ReturnsAsync(containingCategoryBreadcrumb)
                .Verifiable();
            var mixCategoryServiceMock = new Mock<IMixCategoryService>();
            mixCategoryServiceMock.Setup(m => m.GetAllMixCategories())
                .Returns(availableMixCategories.ToAsync())
                .Verifiable();
            mixCategoryServiceMock.Setup(m => m.UpdateMixCategoryAsync(containingCategoryBreadcrumbMixCategory))
                .ReturnsAsync(containingCategoryBreadcrumbMixCategory)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<MixCategory>(containingCategory))
                .Returns(mappedContainingCategory)
                .Verifiable();
            var stringHelperMock = new Mock<IStringHelper>();
            stringHelperMock.Setup(s => s.TransformVietnameseToAscii(containingCategory.Name))
                .Returns(containingCategory.Name)
                .Verifiable();
            var categoryDataDependencyResolver = new CategoryDataDependencyResolver(
                categoryServiceMock.Object, mixCategoryServiceMock.Object,
                mapperMock.Object, stringHelperMock.Object);

            await categoryDataDependencyResolver.UpdateBreadcrumbsOfCategoryAsync(category);

            mixCategoryServiceMock.Verify();
            mapperMock.Verify();
            stringHelperMock.Verify();
            categoryServiceMock.Verify();
        }
        #endregion
    }
}
