using Annstore.Core.Entities.Catalog;
using Annstore.Core.Events;
using Annstore.DataMixture.DataMixtures;
using Annstore.DataMixture.Events;
using Moq;
using System.Threading.Tasks;
using Xunit;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Tests.Events
{
    public class CategoryEventHandlerTests
    {
        #region HandleAsync_CreatedCategory
        [Fact]
        public async Task HandleCreatedCategoryAsync_ApplyForCreatedCategoryAsync()
        {
            var createdCategory = new Category();
            var resultMixCategory = new MixCategory();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForCreatedCategoryAsync(createdCategory))
                .ReturnsAsync(resultMixCategory)
                .Verifiable();
            var categoryEventHandler = new DomainCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryCreatedEvent = new EntityCreatedEvent<Category>(createdCategory);

            await categoryEventHandler.HandleAsync(categoryCreatedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion

        #region HandleAsync_UpdatedCategory
        [Fact]
        public async Task HandleUpdatedCategoryAsync_ApplyForUpdatedCategoryAsync()
        {
            var updatedCategory = new Category();
            var resultMixCategory = new MixCategory();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForUpdatedCategoryAsync(updatedCategory))
                .ReturnsAsync(resultMixCategory)
                .Verifiable();
            var categoryEventHandler = new DomainCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryUpdatedEvent = new EntityUpdatedEvent<Category>(updatedCategory);

            await categoryEventHandler.HandleAsync(categoryUpdatedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion

        #region HandleAsync_DeletedCategory
        [Fact]
        public async Task HandleDeletedCategoryAsync_ApplyForDeletedCategoryAsync()
        {
            var deletedCategory = new Category();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForDeletedCategoryAsync(deletedCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var categoryEventHandler = new DomainCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryDeletedEvent = new EntityDeletedEvent<Category>(deletedCategory);

            await categoryEventHandler.HandleAsync(categoryDeletedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion
    }
}
