using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;
using Annstore.Services;
using Annstore.Services.Catalog;
using AnnstoreShop.Web.Areas.Admin.Controllers;
using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Annstore.Web.Tests.Admin
{
    public class CategoryControllerTests
    {
        [Fact]
        public void Index_RedirectToList()
        {
            var categoryController = new CategoryController(Mock.Of<ICategoryService>());

            var redirectResult = categoryController.Index();

            var listRedirectResult = Assert.IsType<RedirectToActionResult>(redirectResult);
            Assert.Equal(nameof(CategoryController.List), listRedirectResult.ActionName);
        }

        [Fact]
        public async Task List_GetAllCategories()
        {
            var availableCategories = new List<Category> { new Category() { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategories()).ReturnsAsync(availableCategories)
                .Verifiable();
            var categoryController = new CategoryController(categoryServiceMock.Object);

            var result = await categoryController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(availableCategories.Count, ((CategoryListModel) viewResult.Model).Categories.Count);
        }
    }
}
