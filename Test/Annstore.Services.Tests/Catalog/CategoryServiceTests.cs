using System.Linq;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;
using Annstore.Data;
using Annstore.Services.Catalog;
using Moq;
using TestHelper;
using Xunit;

namespace Annstore.Services.Tests.Catalog
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task GetCategories_ReturnAllCategories()
        {
            var availableCategories = new[] {new Category {Id = 1}, new Category {Id = 2}};
            var categoryRepositoryMock = new Mock<IRepository<Category>>();
            categoryRepositoryMock
                .Setup(r => r.Table).Returns(availableCategories.ToAsync())
                .Verifiable();
            var categoryService = new CategoryService(categoryRepositoryMock.Object);
            
            var result = await categoryService.GetCategories();

            Assert.Equal(availableCategories.Length, result.Count);
            categoryRepositoryMock.Verify();
        }
    }
}
