using Annstore.Core.Events;
using Annstore.DataMixture.DataMixtures;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Events
{
    public sealed class MixCategoryEventHandler :
        IEventHandler<EntityCreatedEvent<MixCategory>>,
        IEventHandler<EntityDeletedEvent<MixCategory>>,
        IEventHandler<EntityUpdatedEvent<MixCategory>>
    {
        private readonly ICategoryDataMixturer _categoryDataMixturer;

        public MixCategoryEventHandler(ICategoryDataMixturer categoryDataMixturer)
        {
            _categoryDataMixturer = categoryDataMixturer;
        }

        public async Task HandleAsync(EntityCreatedEvent<MixCategory> eventArgs)
        {
            await _categoryDataMixturer.ApplyForCreatedMixCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }

        public async Task HandleAsync(EntityDeletedEvent<MixCategory> eventArgs)
        {
            await _categoryDataMixturer.ApplyForDeletedMixCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }

        public async Task HandleAsync(EntityUpdatedEvent<MixCategory> eventArgs)
        {
            await _categoryDataMixturer.ApplyForUpdatedMixCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }
    }
}
