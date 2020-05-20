using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using Moq;
using System.Threading.Tasks;
using Xunit;
using TestHelper;
using System.Collections.Generic;
using Annstore.Web.Areas.Admin.Factories;
using AutoMapper;

namespace Annstore.Web.Tests.Factories
{
    public class CategoryModelFactoryTests
    {
        #region PrepareCategoryModelParentCategories
        [Fact]
        public async Task PrepareCategoryModelParentCategories_CategoryModelIdIsZero_ReturnAllCategories()
        {
            var model = new CategoryModel { Id = 0 };
            var allCategories = new List<Category> { new Category { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories)
                .Verifiable();
            var categoryModelFactory = new CategoryModelFactory(categoryServiceMock.Object, Mock.Of<IMapper>());

            var result = await categoryModelFactory.PrepareCategoryModelParentCategories(model);

            Assert.Equal(allCategories.Count, model.ParentableCategories.Count);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategories_CategoryModelIdIsNotZero_ExcludeCurrentCategory()
        {
            var model = new CategoryModel { Id = 1 };
            var allCategories = new List<Category> { new Category { Id = 1 } };
            var categoryServiceMock = new Mock<ICategoryService>();
            categoryServiceMock.Setup(c => c.GetCategoriesAsync())
                .ReturnsAsync(allCategories)
                .Verifiable();
            var categoryModelFactory = new CategoryModelFactory(categoryServiceMock.Object, Mock.Of<IMapper>());

            var result = await categoryModelFactory.PrepareCategoryModelParentCategories(model);

            Assert.Equal(0, model.ParentableCategories.Count);
            categoryServiceMock.Verify();
        }

        [Fact]
        public async Task PrepareCategoryModelParentCategories_ReturnResult_MapToSimpleModel()
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
            var categoryModelFactory = new CategoryModelFactory(categoryServiceStub.Object, mapperMock.Object);

            var result = await categoryModelFactory.PrepareCategoryModelParentCategories(model);

            Assert.Equal(simpleModel, model.ParentableCategories[0]);
            mapperMock.Verify();
        }

        #endregion
    }
}
