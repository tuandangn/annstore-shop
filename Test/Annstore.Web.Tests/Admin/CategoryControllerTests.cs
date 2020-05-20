﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;
using Annstore.Data;
using Annstore.Services.Catalog;
using Annstore.Web.Areas.Admin.Controllers;
using Annstore.Web.Areas.Admin.Factories;
using Annstore.Web.Areas.Admin.Models.Categories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace Annstore.Web.Tests.Admin
{
    public class CategoryControllerTests
    {
        #region List
        [Fact]
        public void Index_RedirectToList()
        {
            var categoryController = new CategoryController(Mock.Of<ICategoryService>(), Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());

            var redirectResult = categoryController.Index();

            var listRedirectResult = Assert.IsType<RedirectToActionResult>(redirectResult);
            Assert.Equal(nameof(CategoryController.List), listRedirectResult.ActionName);
        }
        #endregion

        #region List
        [Fact]
        public async Task List_GetAllCategories()
        {
            var availableCategories = new List<Category> { new Category() { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync()).ReturnsAsync(availableCategories)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());

            var result = await categoryController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(availableCategories.Count, ((CategoryListModel)viewResult.Model).Categories.Count);
        }
        #endregion

        #region Edit

        [Fact]
        public async Task Edit_CategoryNotFound_RedirectToList()
        {
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());

            var result = await categoryController.Edit(0);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_CategoryFound_PrepareValidViewModel()
        {
            var id = 1;
            var category = new Category() { Id = id, DisplayOrder = 1, Name = "Category" };
            var model = new CategoryModel() { Id = category.Id, DisplayOrder = category.DisplayOrder, Name = category.Name, ParentId = category.ParentId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(id))
                .ReturnsAsync(category)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategoryModel>(category))
                .Returns(model)
                .Verifiable();
            var categoryModelFactoryMock = new Mock<ICategoryModelFactory>();
            categoryModelFactoryMock.Setup(cf => cf.PrepareCategoryModelParentCategories(model))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, mapperMock.Object, categoryModelFactoryMock.Object);

            var result = await categoryController.Edit(id);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            mapperMock.Verify();
            categoryServiceMock.Verify();
            categoryModelFactoryMock.Verify();
        }
        #endregion

        #region Edit Post
        [Fact]
        public async Task EditPost_ModelStateIsInvalid_PrepareValidModel()
        {
            var model = new CategoryModel();
            var categoryModelFactoryMock = new Mock<ICategoryModelFactory>();
            categoryModelFactoryMock.Setup(cf => cf.PrepareCategoryModelParentCategories(model))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(Mock.Of<ICategoryService>(), Mock.Of<IMapper>(), categoryModelFactoryMock.Object);
            categoryController.ModelState.AddModelError("error", "error");

            var result = await categoryController.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            categoryModelFactoryMock.Verify();
        }

        [Fact]
        public async Task EditPost_CategoryIsNotFound_RedirectToList()
        {
            var notFoundCategoryId = 1;
            var model = new CategoryModel { Id = notFoundCategoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());

            var result = await categoryController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_CategoryIsFound_UpdateCategory()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var model = new CategoryModel { Id = categoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.UpdateCategoryAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CategoryModel, Category>(model, category))
                .Returns(category)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, mapperMock.Object, Mock.Of<ICategoryModelFactory>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Edit(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
            mapperMock.Verify();
        }
        #endregion

        #region Create
        [Fact]
        public async Task Create_PrepareValidModel()
        {
            var categoryModelFactoryMock = new Mock<ICategoryModelFactory>();
            categoryModelFactoryMock.Setup(cf => cf.PrepareCategoryModelParentCategories(It.IsAny<CategoryModel>()))
                .Verifiable();
            var categoryController = new CategoryController(Mock.Of<ICategoryService>(), Mock.Of<IMapper>(), categoryModelFactoryMock.Object);

            var result = await categoryController.Create();

            Assert.IsType<ViewResult>(result);
            categoryModelFactoryMock.Verify();
        }

        #endregion

        #region Create Post

        [Fact]
        public async Task CreatePost_ModelStateIsInvalid_ReturnView()
        {
            var model = new CategoryModel();
            var categoryModelFactoryMock = new Mock<ICategoryModelFactory>();
            categoryModelFactoryMock.Setup(cf => cf.PrepareCategoryModelParentCategories(model))
                .ReturnsAsync(model)
                .Verifiable();
            var categoryController = new CategoryController(Mock.Of<ICategoryService>(), Mock.Of<IMapper>(), categoryModelFactoryMock.Object);
            categoryController.ModelState.AddModelError("error", "error");

            var result = await categoryController.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            categoryModelFactoryMock.Verify();
        }

        [Fact]
        public async Task CreatePost_ModelStateIsValid_InsertCategory()
        {
            var model = new CategoryModel();
            var category = new Category();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Category>(model))
                .Returns(category)
                .Verifiable();
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.CreateCategoryAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, mapperMock.Object, Mock.Of<ICategoryModelFactory>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
            mapperMock.Verify();
        }
        #endregion

        #region Delete

        [Fact]
        public async Task Delete_CategoryNotFound_RedirectToList()
        {
            var notFoundCategoryId = 1;
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(notFoundCategoryId))
                .ReturnsAsync((Category)null)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());

            var result = await categoryController.Delete(notFoundCategoryId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task Delete_CategoryIsFound_DeleteCategory()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);
            categoryServiceMock.Setup(c => c.DeleteCategoryAsync(category))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object, Mock.Of<IMapper>(), Mock.Of<ICategoryModelFactory>());
            categoryController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await categoryController.Delete(categoryId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.List), redirectToActionResult.ActionName);
            categoryServiceMock.Verify();
        }

        #endregion
    }
}
