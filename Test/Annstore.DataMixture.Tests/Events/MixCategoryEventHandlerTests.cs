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
    public class MixDataEventHandlerTests
    {
        #region HandleAsync_CreatedMixCategory
        [Fact]
        public async Task HandleCreatedMixCategoryAsync_ApplyForCreatedMixCategoryAsync()
        {
            var createdMixCategory = new MixCategory();
            var resultMixCategory = new MixCategory();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForCreatedMixCategoryAsync(createdMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryEventHandler = new MixCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryCreatedEvent = new EntityCreatedEvent<MixCategory>(createdMixCategory);

            await mixCategoryEventHandler.HandleAsync(categoryCreatedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion

        #region HandleAsync_UpdatedMixCategory
        [Fact]
        public async Task HandleUpdatedMixCategoryAsync_ApplyForUpdatedMixCategoryAsync()
        {
            var updatedMixCategory = new MixCategory();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForUpdatedMixCategoryAsync(updatedMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryEventHandler = new MixCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryUpdatedEvent = new EntityUpdatedEvent<MixCategory>(updatedMixCategory);

            await mixCategoryEventHandler.HandleAsync(categoryUpdatedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion

        #region HandleAsync_DeletedMixCategory
        [Fact]
        public async Task HandleDeletedMixCategoryAsync_ApplyForDeletedMixCategoryAsync()
        {
            var deletedMixCategory = new MixCategory();
            var categoryDataMixturerMock = new Mock<ICategoryDataMixturer>();
            categoryDataMixturerMock.Setup(c => c.ApplyForDeletedMixCategoryAsync(deletedMixCategory))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryEventHandler = new MixCategoryEventHandler(categoryDataMixturerMock.Object);
            var categoryDeletedEvent = new EntityDeletedEvent<MixCategory>(deletedMixCategory);

            await mixCategoryEventHandler.HandleAsync(categoryDeletedEvent);

            categoryDataMixturerMock.Verify();
        }

        #endregion
    }
}
