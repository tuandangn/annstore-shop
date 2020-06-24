using Annstore.Core.Entities.Catalog;
using Annstore.Core.Events;
using Annstore.DataMixture.DataMixtures;
using System.Threading.Tasks;

namespace Annstore.DataMixture.Events
{
    public class DomainCategoryEventHandler :
        IEventHandler<EntityCreatedEvent<Category>>,
        IEventHandler<EntityDeletedEvent<Category>>,
        IEventHandler<EntityUpdatedEvent<Category>>
    {
        private readonly ICategoryDataMixturer _categoryDataMixturer;

        public DomainCategoryEventHandler(ICategoryDataMixturer categoryDataMixturer)
        {
            _categoryDataMixturer = categoryDataMixturer;
        }

        public async Task HandleAsync(EntityCreatedEvent<Category> eventArgs)
        {
            await _categoryDataMixturer.ApplyForCreatedCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }

        public async Task HandleAsync(EntityDeletedEvent<Category> eventArgs)
        {
            await _categoryDataMixturer.ApplyForDeletedCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }

        public async Task HandleAsync(EntityUpdatedEvent<Category> eventArgs)
        {
            await _categoryDataMixturer.ApplyForUpdatedCategoryAsync(eventArgs.Entity)
                .ConfigureAwait(false);
        }
    }
}
