using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using AutoMapper;
using System;
using Annstore.Web.Infrastructure;
using Annstore.Web.Areas.Admin.Services.Categories;
using Annstore.Web.Areas.Admin.Services.Categories.Options;

namespace Annstore.Web.Tests.Admin.Services
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
            var allCategories = new List<Category> { new Category { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
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
            var allCategories = new List<Category> { new Category { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
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
            var mappedCategory = new Category { Id = 2 };
            var allCategories = new List<Category> { mappedCategory };
            var simpleModel = new CategorySimpleModel { Id = 2 };
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoriesAsync())
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
            var mappedCategory = new Category { Id = 2, Name = "A" };
            var allCategories = new List<Category> { mappedCategory };
            var simpleModel = new CategorySimpleModel { Id = 2 };
            var categoryBreadcrumb = new List<Category> { new Category { Name = "B" }, mappedCategory };
            var expectedBreadcrum = string.Format("{0} {1} {2}", "B", " - ", "A");
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(mappedCategory, breadcrumbOptions.DeepLevel))
                .ReturnsAsync(categoryBreadcrumb)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(mappedCategory))
                .Returns(simpleModel);
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
                UseParentAsTarget = true
            };
            var model = new CategoryModel { Id = 1 };
            var mappedCategory = new Category { Id = 2, Name = "A", ParentId = 3 };
            var parentCategory = new Category { Id = 3 };
            var allCategories = new List<Category> { mappedCategory };
            var simpleModel = new CategorySimpleModel { Id = 2 };
            var categoryBreadcrumb = new List<Category> { new Category { Name = "B" }, mappedCategory };
            var expectedBreadcrum = string.Format("{0} {1} {2}", "B", " - ", "A");
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, breadcrumbOptions.DeepLevel))
                .ReturnsAsync(categoryBreadcrumb);
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(mappedCategory.ParentId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(mappedCategory))
                .Returns(simpleModel);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperStub.Object);

            var result = await adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions);

            Assert.Equal(expectedBreadcrum, result.ParentableCategories[0].Breadcrumb);
            categoryServiceMock.Verify();
        }

        #endregion

        #region GetCategoryModelAsync
        [Fact]
        public async Task GetCategoryModelAsync_CategoryNotFound_ReturnNull()
        {
            var notFoundCategoryId = 0;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var nullModel = await adminCategoryService.GetCategoryModelAsync(notFoundCategoryId, new BreadcrumbOptions());

            Assert.Null(nullModel);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryModelAsync_CategoryNotNull_MapToCategoryModel()
        {
            var id = 1;
            var category = new Category { Id = id };
            var model = new CategoryModel { Id = id };
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategoryModel>(category))
                .Returns(model)
                .Verifiable();
            var categoryServiceStub = new Mock<ICategoryService>();
            categoryServiceStub.Setup(c => c.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);
            categoryServiceStub.Setup(c => c.GetCategoriesAsync())
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
            var category = new Category { Id = id };
            var model = new CategoryModel { Id = id };
            var allCategories = new List<Category> { category };
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategoryModel>(category))
                .Returns(model);
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
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
            var category = new Category { Id = 1 };
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategorySimpleModel>(category))
                .Returns(mappedModel)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, mapperMock.Object);
            var categorySettings = new CategoryListOptions();

            var categoryListModel = await adminCategoryService.GetCategoryListModelAsync(categorySettings);

            Assert.Single(categoryListModel.Categories);
            Assert.Equal(mappedModel, categoryListModel.Categories[0]);
            categoryServiceMock.Verify();
            mapperMock.Verify();
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
                    Separator = separator
                }
            };
            var category = new Category { Id = 1, Name = "A" };
            var categoryBreadcrumb = new List<Category> { new Category { Name = "B" }, category };
            var expectedBreadcrumb = "B | A";
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category, deepLevel))
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
            CategoryListOptions categoryListOptions = new CategoryListOptions
            {
                Breadcrumb = new BreadcrumbOptions
                {
                    Enable = true,
                    DeepLevel = deepLevel,
                    Separator = separator,
                    UseParentAsTarget = parentOnly
                }
            };
            var category = new Category { Id = 1, Name = "A", ParentId = 2 };
            var parentCategory = new Category { Id = 2, Name = "B" };
            var categoryBreadcrumb = new List<Category> { new Category { Name = "C" }, new Category { Name = "D" } };
            var expectedBreadcrumb = "C | D";
            var mappedModel = new CategorySimpleModel { Id = category.Id };
            var allCategories = new List<Category> { category };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories);
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, deepLevel))
                .ReturnsAsync(categoryBreadcrumb);
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategorySimpleModel>(category))
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
            var category = new Category { Id = categoryModel.Id };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.UpdateCategoryAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategoryModel, Category>(categoryModel, category))
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
        public async Task UpdateCategoryAsync_UpdateCategoryError_ReturnErrorResponse()
        {
            var categoryModel = new CategoryModel { Id = 2 };
            var category = new Category { Id = categoryModel.Id };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryModel.Id))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.UpdateCategoryAsync(category))
                .Throws<Exception>()
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<CategoryModel, Category>(categoryModel, category))
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
            var category = new Category { Id = categoryModel.Id };
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
            var category = new Category { Id = categoryModel.Id };
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
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(category, deepLevel))
                .ReturnsAsync(breadcrumb)
                .Verifiable();
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, false);

            Assert.Equal(string.Format("{0} {1} {2}", breadcrumb[0].Name, separator, breadcrumb[1].Name), breadcrumbString);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task GetCategoryBreadcrumbStringAsync_UseParentAsTarget_ReturnParentBreadcrumb()
        {
            var useParentAsTarget = true; 
            var category = new Category { ParentId = 1 };
            var parentCategory = new Category { Id = 1 };
            var separator = ">>>";
            var deepLevel = 2;
            var parentBreadcrumb = new List<Category> { new Category { Name = "A" }, new Category { Name = "B" } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(category.ParentId))
                .ReturnsAsync(parentCategory)
                .Verifiable();
            categoryServiceMock.Setup(c => c.GetCategoryBreadcrumbAsync(parentCategory, deepLevel))
                .ReturnsAsync(parentBreadcrumb);
            var adminCategoryService = new AdminCategoryService(categoryServiceMock.Object, Mock.Of<IMapper>());

            var breadcrumbString = await adminCategoryService.GetCategoryBreadcrumbStringAsync(category, deepLevel, separator, useParentAsTarget);

            Assert.Equal(string.Format("{0} {1} {2}", parentBreadcrumb[0].Name, separator, parentBreadcrumb[1].Name), breadcrumbString);
            categoryServiceMock.Verify();
        }

        #endregion
    }
}
