using Annstore.Core.Events;
using Annstore.DataMixture.Services.Catalog;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Tests.Services
{
    public class MixCategoryServiceTests
    {
        #region GetMixCategoryByEntityIdAsync
        [Fact]
        public async Task GetMixCategoryByEntityIdAsync()
        {
            var entityId = 1;
            var resultMixCategory = new MixCategory { EntityId = entityId };
            var mixCategoryRepositoryMock = new Mock<IMixRepository<MixCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.FindByEntityIdAsync(entityId))
                .ReturnsAsync(resultMixCategory)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(mixCategoryRepositoryMock.Object, Mock.Of<IEventPublisher>());

            var result = await mixCategoryService.GetMixCategoryByEntityIdAsync(entityId);

            Assert.Equal(resultMixCategory, result);
            mixCategoryRepositoryMock.Verify();
        }

        #endregion

        #region CreateMixCategoryAsync
        [Fact]
        public async Task CreateMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<IMixRepository<MixCategory>>(), Mock.Of<IEventPublisher>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.CreateMixCategoryAsync(null));
        }

        [Fact]
        public async Task CreateMixCategoryAsync_CategoryIsNotNull_InsertCategoryAndPublishEntityCreatedEvent()
        {
            var category = new MixCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<MixCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.InsertAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var eventPublisherMock = new Mock<IEventPublisher>();
            eventPublisherMock.Setup(p => p.PublishAsync(It.IsAny<EntityCreatedEvent<MixCategory>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(mixCategoryRepositoryMock.Object, eventPublisherMock.Object);

            var result = await mixCategoryService.CreateMixCategoryAsync(category);

            Assert.Equal(category, result);
            mixCategoryRepositoryMock.Verify();
            eventPublisherMock.Verify();
        }
        #endregion

        #region DeleteMixCategoryAsync
        [Fact]
        public async Task DeleteMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<IMixRepository<MixCategory>>(), Mock.Of<IEventPublisher>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.DeleteMixCategoryAsync(null));
        }

        [Fact]
        public async Task DeleteMixCategoryAsync_CategoryIsNotNull_DeleteCategoryAndPublishEntityDeletedEvent()
        {
            var category = new MixCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<MixCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.DeleteAsync(category))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var eventPublisherMock = new Mock<IEventPublisher>();
            eventPublisherMock.Setup(p => p.PublishAsync(It.IsAny<EntityDeletedEvent<MixCategory>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(mixCategoryRepositoryMock.Object, eventPublisherMock.Object);

            await mixCategoryService.DeleteMixCategoryAsync(category);

            mixCategoryRepositoryMock.Verify();
            eventPublisherMock.Verify();
        }
        #endregion

        #region UpdateMixCategoryAsync
        [Fact]
        public async Task UpdateMixCategoryAsync_CategoryIsNull_ThrowArgumentNullException()
        {
            var mixCategoryService = new MixCategoryService(Mock.Of<IMixRepository<MixCategory>>(), Mock.Of<IEventPublisher>());
            await Assert.ThrowsAsync<ArgumentNullException>(() => mixCategoryService.UpdateMixCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateMixCategoryAsync_CategoryIsNotNull_UpdateCategoryAndPublishEntityUpdatedEvent()
        {
            var category = new MixCategory();
            var mixCategoryRepositoryMock = new Mock<IMixRepository<MixCategory>>();
            mixCategoryRepositoryMock.Setup(r => r.UpdateAsync(category))
                .ReturnsAsync(category)
                .Verifiable();
            var eventPublisherMock = new Mock<IEventPublisher>();
            eventPublisherMock.Setup(p => p.PublishAsync(It.IsAny<EntityUpdatedEvent<MixCategory>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var mixCategoryService = new MixCategoryService(mixCategoryRepositoryMock.Object, eventPublisherMock.Object);

            var result = await mixCategoryService.UpdateMixCategoryAsync(category);

            Assert.Equal(category, result);
            mixCategoryRepositoryMock.Verify();
            eventPublisherMock.Verify();
        }
        #endregion
    }
}
