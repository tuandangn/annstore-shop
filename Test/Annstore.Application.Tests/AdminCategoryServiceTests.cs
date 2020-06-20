using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using AutoMapper;
using System;
using Annstore.Core.Common;
using Annstore.Application.Services.Categories;
using Annstore.Application.Models.Admin.Categories;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Infrastructure;

namespace Annstore.Application.Tests
{
    public class AdminCategoryServiceTests
    {
        #region PrepareCategoryModelParentCategoriesAsync
        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.PrepareCategoryModelParentCategoriesAsync(null, null));
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_CategoryModelIdIsZero_ReturnAllCategories()
        {
            var model = new CategoryModel { Id = 0 };
            var allCategories = new List<Category> { Category.CreateWithId(1) };
            var showHidden = false;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(allCategories)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, new BreadcrumbOptions());

            Assert.Equal(allCategories.Count, model.ParentableCategories.Count);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_CategoryModelIdIsNotZero_ExcludeCurrentCategory()
        {
            var model = new CategoryModel { Id = 1 };
            var allCategories = new List<Category> { Category.CreateWithId(1) };
            var showHidden = false;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(allCategories)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, new BreadcrumbOptions());

            Assert.Equal(0, model.ParentableCategories.Count);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_ReturnResult_MapToSimpleModel()
        {
            var model = new CategoryModel { Id = 1 };
            var mappedCategory = Category.CreateWithId(2);
            var allCategories = new List<Category> { mappedCategory };
            var simpleModel = new CategorySimpleModel { Id = 2 };
            var showHidden = false;
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(allCategories);
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategorySimpleModel>(mappedCategory))
                .Returns(simpleModel)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceStub.Object, mapperMock.Object);

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, new BreadcrumbOptions());

            Assert.Equal(simpleModel, model.ParentableCategories[0]);
            mapperMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_BreadcrumbEnable_IncludeCategoryBreadcrumb()
        {
            var breadcrumbOptions = new BreadcrumbOptions
            {
                Enable = true,
                DeepLevel = 3,
                Separator = " - ",
                UseParentAsTarget = false
            };
            var model = new CategoryModel { Id = 1 };
            var modelMappedCategory = Category.CreateWithId(1);
            var parentableCategory = Category.CreateWithId(2);
            parentableCategory.Name = "A";
            parentableCategory.ParentId = 3;
            var availableCategories = new List<Category> { modelMappedCategory, parentableCategory };
            var parentableSimpleModel = new CategorySimpleModel { Id = 2 };
            var parentableCategoryBreadcrumb = new List<Category> { new Category { Name = "B" }, parentableCategory };
            var expectedBreadcrum = string.Format("{0} {1} {2}", "B", " - ", "A");
            var showHidden = false;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(availableCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(parentableCategory, breadcrumbOptions.DeepLevel, showHidden))
                .ReturnsAsync(parentableCategoryBreadcrumb)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(parentableCategory))
                .Returns(parentableSimpleModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions);

            Assert.Equal(expectedBreadcrum, result.ParentableCategories[0].Breadcrumb);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategoriesAsync_BreadcrumbUseParentAsTarget_IncludeParentCategoryBreadcrumb()
        {
            var breadcrumbOptions = new BreadcrumbOptions
            {
                Enable = true,
                DeepLevel = 3,
                Separator = " - ",
                UseParentAsTarget = true,
                ShowHidden = true
            };
            var model = new CategoryModel { Id = 1 };
            var category2WithParentIdIs3 = Category.CreateWithId(2);
            category2WithParentIdIs3.ParentId = 3;
            category2WithParentIdIs3.Name = "A";
            var category3WithParentIdIs0 = Category.CreateWithId(3);
            var allCategories = new List<Category> { category2WithParentIdIs3 };
            var simpleModel = new CategorySimpleModel { Id = 2 };
            var categoryBreadcrumb = new List<Category> { new Category { Name = "B" }, category2WithParentIdIs3 };
            var expectedBreadcrum = string.Format("{0} {1} {2}", "B", " - ", "A");
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(allCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category3WithParentIdIs0, breadcrumbOptions.DeepLevel, breadcrumbOptions.ShowHidden))
                .ReturnsAsync(categoryBreadcrumb);
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category2WithParentIdIs3.ParentId))
                .ReturnsAsync(category3WithParentIdIs0)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(category2WithParentIdIs3))
                .Returns(simpleModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions, showHidden);

            Assert.Equal(expectedBreadcrum, result.ParentableCategories[0].Breadcrumb);
            categoryServiceMock.Verify();
        }

        #endregion

        #region GetCategoryModelAsync
        [Fact]
        public async Task GetCategoryModelAsync_CategoryNotFound_ReturnNullCategoryModel()
        {
            var notFoundCategoryId = 0;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var nullCategoryModel = await adminCategoryService.GetCategoryModelAsync(notFoundCategoryId, new BreadcrumbOptions());

            Assert.IsType<NullCategoryModel>(nullCategoryModel);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryModelAsync_CategoryDeleted_ReturnNullCategoryModel()
        {
            var deletedCategoryId = 1;
            var deletedCategory = Category.CreateWithId(deletedCategoryId);
            deletedCategory.IsDeleted(true);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(deletedCategoryId))
                .ReturnsAsync(deletedCategory)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var nullCategoryModel = await adminCategoryService.GetCategoryModelAsync(deletedCategoryId, new BreadcrumbOptions());

            Assert.IsType<NullCategoryModel>(nullCategoryModel);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryModelAsync_CategoryNotNull_MapToCategoryModel()
        {
            var id = 1;
            var category = Category.CreateWithId(id);
            var model = new CategoryModel { Id = id };
            var showHidden = true;
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategoryModel>(category))
                .Returns(model)
                .Verifiable();
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);
            categoryServiceStub.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(new List<Category>());
            var adminCategoryService = new AdminCategoryService(categoryServiceStub.Object, mapperMock.Object);

            var result = await adminCategoryService.GetCategoryModelAsync(id, new BreadcrumbOptions());

            Assert.Equal(model, result);
            mapperMock.Verify();
        }

        [Fact]
        public async Task GetCategoryModelAsync_MappedModel_AddExcludeCurrentCategoryForParentCategories()
        {
            var id = 1;
            var category = Category.CreateWithId(id);
            var model = new CategoryModel { Id = id };
            var allCategories = new List<Category> { category };
            var showHidden = true;
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategoryModel>(category))
                .Returns(model);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.GetCategoriesAsync(showHidden))
                .ReturnsAsync(allCategories)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var result = await adminCategoryService.GetCategoryModelAsync(id, new BreadcrumbOptions());

            Assert.Empty(result.ParentableCategories);
            categoryServiceMock.Verify();
        }

        #endregion

        #region GetCategoryListModelAsync
        [Fact]
        public async Task GetCategoryListModelAsync_ReturnMappedAllCategoriesListModel()
        {
            var category = Category.CreateWithId(1);
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var categoryListOptions = new CategoryListOptions { PageNumber = 1, PageSize = int.MaxValue };
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetPagedCategoriesAsync(categoryListOptions.PageNumber, categoryListOptions.PageSize, showHidden))
                .ReturnsAsync(allCategories.ToPagedList(categoryListOptions.PageSize, categoryListOptions.PageNumber, 12))
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategorySimpleModel>(category))
                .Returns(mappedModel)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperMock.Object);

            var categoryListModel = await adminCategoryService.GetCategoryListModelAsync(categoryListOptions);

            Assert.Single(categoryListModel.Categories);
            Assert.Equal(mappedModel, categoryListModel.Categories[0]);
            categoryServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task GetCategoryListModelAsync_Pagination_ReturnPagedCategories()
        {
            var category = Category.CreateWithId(2);
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var totalItems = 3;
            var categoryListOptions = new CategoryListOptions { PageNumber = 2, PageSize = 1 };
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetPagedCategoriesAsync(categoryListOptions.PageNumber, categoryListOptions.PageSize, showHidden))
                .ReturnsAsync(allCategories.ToPagedList(categoryListOptions.PageSize, categoryListOptions.PageNumber, totalItems))
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(category))
                .Returns(mappedModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var categoryListModel = await adminCategoryService.GetCategoryListModelAsync(categoryListOptions);

            Assert.Equal(3, categoryListModel.TotalPages);
            Assert.Equal(3, categoryListModel.TotalItems);
            Assert.Equal(2, categoryListModel.Categories[0].Id);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryListModelAsync_BreadcrumbIsEnabled_PrepareBreadcrumb()
        {
            var separator = "|";
            var deepLevel = 9;
            CategoryListOptions categoryListOptions = new CategoryListOptions
            {
                Breadcrumb = new BreadcrumbOptions
                {
                    Enable = true,
                    DeepLevel = deepLevel,
                    Separator = separator,
                    ShowHidden = true
                },
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            var category = Category.CreateWithId(1);
            category.Name = "A";
            var categoryBreadcrumb = new List<Category> { new Category { Name = "B" }, category };
            var expectedBreadcrumb = "B | A";
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetPagedCategoriesAsync(categoryListOptions.PageNumber, categoryListOptions.PageSize, showHidden))
                .ReturnsAsync(allCategories.ToPagedList(categoryListOptions.PageSize, categoryListOptions.PageNumber, 12));
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category, deepLevel, categoryListOptions.Breadcrumb.ShowHidden))
                .ReturnsAsync(categoryBreadcrumb)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(category))
                .Returns(mappedModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var categoryListModel = await adminCategoryService.GetCategoryListModelAsync(categoryListOptions);

            Assert.Equal(expectedBreadcrumb, categoryListModel.Categories[0].Breadcrumb);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryListModelAsync_BreadcrumbUseParentAsTarget_GetParentCategoryBreadcrumbIfAny()
        {
            var separator = "|";
            var deepLevel = 9;
            var parentOnly = true;
            var categoryListOptions = new CategoryListOptions
            {
                Breadcrumb = new BreadcrumbOptions
                {
                    Enable = true,
                    DeepLevel = deepLevel,
                    Separator = separator,
                    UseParentAsTarget = parentOnly,
                    ShowHidden = true
                },
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            var category1WithParentIdIs2 = Category.CreateWithId(1);
            category1WithParentIdIs2.ParentId = 2;
            category1WithParentIdIs2.Name = "A";
            var category2WithParentIdIs0 = Category.CreateWithId(2);
            category2WithParentIdIs0.Name = "B";
            var categoryBreadcrumb = new List<Category> { new Category { Name = "C" }, new Category { Name = "D" } };
            var expectedBreadcrumb = "C | D";
            var mappedModel = new CategorySimpleModel { Id = category1WithParentIdIs2.Id };
            var allCategories = new List<Category> { category1WithParentIdIs2 };
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category1WithParentIdIs2.ParentId))
                .ReturnsAsync(category2WithParentIdIs0)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetPagedCategoriesAsync(categoryListOptions.PageNumber, categoryListOptions.PageSize, showHidden))
                .ReturnsAsync(allCategories.ToPagedList(categoryListOptions.PageSize, categoryListOptions.PageNumber, 12));
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category2WithParentIdIs0, deepLevel, categoryListOptions.Breadcrumb.ShowHidden))
                .ReturnsAsync(categoryBreadcrumb);
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(category1WithParentIdIs2))
                .Returns(mappedModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var categoryListModel = await adminCategoryService.GetCategoryListModelAsync(categoryListOptions);

            Assert.Equal(expectedBreadcrumb, categoryListModel.Categories[0].Breadcrumb);
            categoryServiceMock.Verify();
        }

        #endregion

        #region CreateCategoryAsync
        [Fact]
        public async Task CreateCategoryAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.CreateCategoryAsync(null));
        }

        [Fact]
        public async Task CreateCategoryAsync_RequestNotNull_InsertCategory()
        {
            var categoryModel = new CategoryModel();
            var category = new Category();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.CreateCategoryAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Category>(categoryModel))
                .Returns(category)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperMock.Object);
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.CreateCategoryAsync(appRequest);

            Assert.True(appResponse.Success);
            Assert.Equal(category, appResponse.Result);
            categoryServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task CreateCategoryAsync_InsertCategoryError_ReturnErrorResponse()
        {
            var categoryModel = new CategoryModel();
            var category = new Category();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.CreateCategoryAsync(category))
                .Throws<Exception>()
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<Category>(categoryModel))
                .Returns(category);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.CreateCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            categoryServiceMock.Verify();
        }

        #endregion

        #region UpdateCategoryAsync
        [Fact]
        public async Task UpdateCategoryAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.UpdateCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryIsFound_UpdateCategory()
        {
            var categoryModel = new CategoryModel { Id = 1 };
            var category = Category.CreateWithId(categoryModel.Id);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.UpdateCategoryAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(categoryModel, category))
                .Returns(category)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperMock.Object);
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.UpdateCategoryAsync(appRequest);

            Assert.True(appResponse.Success);
            Assert.Equal(category, appResponse.Result);
            categoryServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryNotFound_ReturnModelInvalidResponse()
        {
            var notFoundCategoryId = 0;
            var categoryModel = new CategoryModel { Id = notFoundCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.UpdateCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryIsDeleted_ReturnModelInvalidResponse()
        {
            var deletedCategoryId = 1;
            var deletedCategory = Category.CreateWithId(deletedCategoryId);
            deletedCategory.IsDeleted(true);
            var categoryModel = new CategoryModel { Id = deletedCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(deletedCategory)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.UpdateCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdateCategoryError_ReturnErrorResponse()
        {
            var categoryModel = new CategoryModel { Id = 2 };
            var category = Category.CreateWithId(categoryModel.Id);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.UpdateCategoryAsync(category))
                .Throws<Exception>()
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map(categoryModel, category))
                .Returns(category);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);
            var appRequest = new AppRequest<CategoryModel>(categoryModel);

            var appResponse = await adminCategoryService.UpdateCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            categoryServiceMock.Verify();
        }

        #endregion

        #region DeleteCategoryAsync

        [Fact]
        public async Task DeleteCategoryAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.DeleteCategoryAsync(null));
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryIsFound_DeleteCategory()
        {
            var id = 1;
            var categoryModel = new CategoryModel { Id = id };
            var category = Category.CreateWithId(id);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.DeleteCategoryAsync(category))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(id);

            var appResponse = await adminCategoryService.DeleteCategoryAsync(appRequest);

            Assert.True(appResponse.Success);
            categoryServiceMock.Verify();
        }
        [Fact]
        public async Task DeleteCategoryAsync_CategoryIsDeleted_SkipToSuccess()
        {
            var deletedCategoryId = 1;
            var categoryModel = new CategoryModel { Id = deletedCategoryId };
            var deletedCategory = Category.CreateWithId(deletedCategoryId);
            deletedCategory.IsDeleted(true);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(deletedCategory);
            categoryServiceMock.Setup(c => c.DeleteCategoryAsync(deletedCategory))
                .Returns(Task.CompletedTask);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(deletedCategoryId);

            var appResponse = await adminCategoryService.DeleteCategoryAsync(appRequest);

            Assert.True(appResponse.Success);
            categoryServiceMock.Verify(c => c.DeleteCategoryAsync(deletedCategory), Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryNotFound_ReturnModelInvalidResponse()
        {
            var notFoundCategoryId = 0;
            var categoryModel = new CategoryModel { Id = notFoundCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(notFoundCategoryId);

            var appResponse = await adminCategoryService.DeleteCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.True(appResponse.ModelIsInvalid);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task DeleteCategoryAsync_DeleteCategoryError_ReturnErrorResponse()
        {
            var id = 2;
            var categoryModel = new CategoryModel { Id = id };
            var category = Category.CreateWithId(id);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.DeleteCategoryAsync(category))
                .Throws<Exception>()
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());
            var appRequest = new AppRequest<int>(id);

            var appResponse = await adminCategoryService.DeleteCategoryAsync(appRequest);

            Assert.False(appResponse.Success);
            Assert.NotNull(appResponse.Message);
            categoryServiceMock.Verify();
        }

        #endregion

        #region GetCategoryBreadcrumbStringAsync
        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.GetCategoryBreadcrumbStringAsync(null, 0, string.Empty, false));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_SeparatorIsNull_ThrowArgumentNullException()
        {
            var adminCategoryService = new AdminCategoryService(Mock.Of<ICategoryService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminCategoryService.GetCategoryBreadcrumbStringAsync(null, 0, null, false));
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_ReturnValidBreadcrumb()
        {
            var category = new Category();
            var separator = ">>>";
            var deepLevel = 2;
            var breadcrumb = new List<Category> { new Category { Name = "A" }, new Category { Name = "B" } };
            var showHidden = false;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category, deepLevel, showHidden))
                .ReturnsAsync(breadcrumb)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, showHidden);

            Assert.Equal(string.Format("{0} {1} {2}", breadcrumb[0].Name, separator, breadcrumb[1].Name), breadcrumbString);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_UseParentAsTarget_ReturnParentBreadcrumb()
        {
            var useParentAsTarget = true;
            var category = new Category { ParentId = 1 };
            var parentCategory = Category.CreateWithId(1);
            var separator = ">>>";
            var deepLevel = 2;
            var parentBreadcrumb = new List<Category> { new Category { Name = "A" }, new Category { Name = "B" } };
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, deepLevel, showHidden))
                .ReturnsAsync(parentBreadcrumb);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, useParentAsTarget, showHidden);

            Assert.Equal(string.Format("{0} {1} {2}", parentBreadcrumb[0].Name, separator, parentBreadcrumb[1].Name), breadcrumbString);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_ParentIsDeleted_ReturnEmptyResult()
        {
            var useParentAsTarget = true;
            var category = new Category { ParentId = 1 };
            var deletedParentCategory = Category.CreateWithId(1);
            deletedParentCategory.IsDeleted(true);
            var separator = ">>>";
            var deepLevel = 2;
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(deletedParentCategory)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, useParentAsTarget, showHidden);

            Assert.Empty(breadcrumbString);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_ParentIsNull_ReturnEmptyResult()
        {
            var useParentAsTarget = true;
            var category = new Category { ParentId = 1 };
            var separator = ">>>";
            var deepLevel = 2;
            var showHidden = true;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, useParentAsTarget, showHidden);

            Assert.Empty(breadcrumbString);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_ShowHiddenIsFalseAndParentIsNotPublished_ReturnEmptyResult()
        {
            var useParentAsTarget = true;
            var category = new Category { ParentId = 1 };
            var unpublishedParentCategory = Category.CreateWithId(category.ParentId);
            unpublishedParentCategory.Published = false;
            var separator = ">>>";
            var deepLevel = 2;
            var showHidden = false;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(unpublishedParentCategory)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, useParentAsTarget, showHidden);

            Assert.Empty(breadcrumbString);
            categoryServiceMock.Verify();
        }

        #endregion
    }
}
