using System.Threading.Tasks;
using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Application.Models.Admin.Categories;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Services.Categories;
using Annstore.Core.Entities.Catalog;
using Annstore.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Annstore.Web.Tests.Admin
{
    public class CategoryControllerTests
    {
        #region Index
        [Fact]
        public void Index_RedirectToList()
        {
            var categoryController = new CategoryController(Mock.Of<IAdminCategoryService>(), Mock.Of<IOptionsSnapshot<CategorySettings>>());

            var redirectResult = categoryController.Index();

            var listRedirectResult = Assert.IsType<RedirectToActionResult>(redirectResult);
            Assert.Equal(nameof(CategoryController.List), listRedirectResult.ActionName);
        }
        #endregion

        #region List
        [Fact]
        public async Task List_ReturnValidModel()
        {
            var categoryListModel = new CategoryListModel();
            var categorySettings = new CategorySettings();
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.GetCategoryListModelAsync(It.IsAny<CategoryListOptions>()))
                .ReturnsAsync(categoryListModel)
                .Verifiable();
            var categorySettingsSnapshopStub = new Mock<IOptionsSnapshot<CategorySettings>>();
            categorySettingsSnapshopStub.Setup(csn => csn.Value)
                .Returns(categorySettings)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, categorySettingsSnapshopStub.Object);

            var result = await categoryController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(categoryListModel, viewResult.Model);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task List_PaginationInfoInvalid_UseDefaultValues()
        {
            var page = 0;
            var size = -1;
            var categoryListModel = new CategoryListModel();
            var categorySettings = new CategorySettings { Admin = new CategorySettings.AdminCategorySettings { DefaultPageSize = 12 } };
            var adminCategoryServiceStub = new Mock<IAdminCategoryService>();
            adminCategoryServiceStub.Setup(c =>
            c.GetCategoryListModelAsync(It.Is<CategoryListOptions>(opts =>
                opts.PageSize == 12 && opts.PageNumber == 1
            ))).ReturnsAsync(categoryListModel);
            var categorySettingsSnapshopStub = new Mock<IOptionsSnapshot<CategorySettings>>();
            categorySettingsSnapshopStub.Setup(csn => csn.Value)
                .Returns(categorySettings)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceStub.Object, categorySettingsSnapshopStub.Object);

            var result = await categoryController.List(page, size);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(categoryListModel, viewResult.Model);
        }
        #endregion

        #region Edit

        [Fact]
        public async Task Edit_ModelIsNull_RedirectToList()
        {
            var notFoundCategoryId = 0;
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(s => s.GetCategoryModelAsync(notFoundCategoryId, It.IsAny<BreadcrumbOptions>()))
                .ReturnsAsync((CategoryModel)null)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());

            var result = await categoryController.Edit(0);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_CategoryFound_PrepareValidViewModel()
        {
            var id = 1;
            var model = new CategoryModel();
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(s => s.GetCategoryModelAsync(id, It.IsAny<BreadcrumbOptions>()))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());

            var result = await categoryController.Edit(id);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            adminCategoryServiceMock.Verify();
        }
        #endregion

        #region Edit Post
        [Fact]
        public async Task EditPost_ModelStateIsInvalid_PrepareValidModel()
        {
            var model = new CategoryModel();
            var showHidden = true;
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(cf => cf.PrepareCategoryModelParentCategoriesAsync(model, It.IsAny<BreadcrumbOptions>(), showHidden))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());
            categoryController.ModelState.AddModelError("error", "error");

            var result = await categoryController.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_InvalidModelResponse_RedisplayView()
        {
            var notFoundCategoryId = 1;
            var model = new CategoryModel { Id = notFoundCategoryId };
            var invalidModelResponse = AppResponse.InvalidModelResult<Category>(string.Empty);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.UpdateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_ErrorResponse_RedisplayViewWithValidModel()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var model = new CategoryModel { Id = categoryId };
            var showHidden = true;
            var errorResponse = AppResponse.ErrorResult<Category>(string.Empty);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.UpdateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            adminCategoryServiceMock.Setup(c => c.PrepareCategoryModelParentCategoriesAsync(model, It.IsAny<BreadcrumbOptions>(), showHidden))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_SuccessResponse_RedirectToList()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var model = new CategoryModel { Id = categoryId };
            var successResponse = AppResponse.SuccessResult(category);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.UpdateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }
        #endregion

        #region Create
        [Fact]
        public async Task Create_PrepareValidModel()
        {
            var showHidden = true;
            var categoryModelFactoryMock = new Mock<IAdminCategoryService>();
            categoryModelFactoryMock.Setup(cf => cf.PrepareCategoryModelParentCategoriesAsync(It.IsAny<CategoryModel>(), It.IsAny<BreadcrumbOptions>(), showHidden))
                .Verifiable();
            var categoryController = new CategoryController(categoryModelFactoryMock.Object, _GetDefaultCategorySettingsSnapshot());

            var result = await categoryController.Create();

            Assert.IsType<ViewResult>(result);
            categoryModelFactoryMock.Verify();
        }

        #endregion

        #region Create Post

        [Fact]
        public async Task CreatePost_ModelStateIsInvalid_RedisplayViewWithValidModel()
        {
            var model = new CategoryModel();
            var showHidden = true;
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(cf => cf.PrepareCategoryModelParentCategoriesAsync(model, It.IsAny<BreadcrumbOptions>(), showHidden))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());
            categoryController.ModelState.AddModelError("error", "error");

            var result = await categoryController.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_ErrorResponse_RedisplayViewWithValidModel()
        {
            var model = new CategoryModel();
            var showHidden = true;
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            var errorResponse = AppResponse.ErrorResult<Category>(string.Empty);
            adminCategoryServiceMock.Setup(cf => cf.PrepareCategoryModelParentCategoriesAsync(model, It.IsAny<BreadcrumbOptions>(), showHidden))
                .ReturnsAsync(model)
                .Verifiable();
            adminCategoryServiceMock.Setup(cf => cf.CreateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, _GetDefaultCategorySettingsSnapshot());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_InvalidModelResponse_RedirectToList()
        {
            var model = new CategoryModel();
            var category = new Category();
            var invalidModelResponse = AppResponse.InvalidModelResult<Category>(string.Empty);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.CreateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_SuccessResponse_RedirectToList()
        {
            var model = new CategoryModel();
            var category = new Category();
            var successResponse = AppResponse.SuccessResult(category);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.CreateCategoryAsync(It.Is<AppRequest<CategoryModel>>(req => req.Data == model)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }
        #endregion

        #region Delete

        [Fact]
        public async Task Delete_InvalidModelResponse_RedirectToList()
        {
            var notFoundCategoryId = 1;
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            var invalidModelResponse = AppResponse.InvalidModelResult<Category>(string.Empty);
            adminCategoryServiceMock.Setup(c => c.DeleteCategoryAsync(It.Is<AppRequest<int>>(req => req.Data == notFoundCategoryId)))
                .ReturnsAsync(invalidModelResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Delete(notFoundCategoryId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task Delete_ErrorResponse_RedirectToList()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var errorResponse = AppResponse.ErrorResult<Category>(string.Empty);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.DeleteCategoryAsync(It.Is<AppRequest<int>>(req => req.Data == categoryId)))
                .ReturnsAsync(errorResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Delete(categoryId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        [Fact]
        public async Task Delete_SuccessResponse_RedirectToList()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var successResponse = AppResponse.SuccessResult(category);
            var adminCategoryServiceMock = new Mock<IAdminCategoryService>();
            adminCategoryServiceMock.Setup(c => c.DeleteCategoryAsync(It.Is<AppRequest<int>>(req => req.Data == categoryId)))
                .ReturnsAsync(successResponse)
                .Verifiable();
            var categoryController = new CategoryController(adminCategoryServiceMock.Object, Mock.Of<IOptionsSnapshot<CategorySettings>>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Delete(categoryId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            adminCategoryServiceMock.Verify();
        }

        #endregion

        #region Helpers
        private IOptionsSnapshot<CategorySettings> _GetDefaultCategorySettingsSnapshot()
        {
            var categorySettingsSnapshotStub = new Mock<IOptionsSnapshot<CategorySettings>>();
            categorySettingsSnapshotStub.Setup(c => c.Value)
                .Returns(new CategorySettings());

            return categorySettingsSnapshotStub.Object; ;
        }
        #endregion
    }
}
