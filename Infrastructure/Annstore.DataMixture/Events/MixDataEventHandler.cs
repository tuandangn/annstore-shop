using Annstore.Core.Entities.Catalog;
using Annstore.Core.Events;
using Annstore.DataMixture.Services.Catalog;
using System.Threading.Tasks;

namespace Annstore.DataMixture.Events
{
    public class MixDataEventHandler :
        IEventHandler<EntityCreatedEvent<Category>>,
        IEventHandler<EntityDeletedEvent<Category>>,
        IEventHandler<EntityUpdatedEvent<Category>>
    {
        private readonly IMixCategoryService _mixCategoryService;

        public MixDataEventHandler(IMixCategoryService mixCategoryService)
        {
            _mixCategoryService = mixCategoryService;
        }

        public async Task HandleAsync(EntityCreatedEvent<Category> eventArgs)
        {
            await _mixCategoryService.CreateMixCategoryAsync(eventArgs.Entity);
        }

        public async Task HandleAsync(EntityDeletedEvent<Category> eventArgs)
        {
            await _mixCategoryService.DeleteMixCategoryAsync(eventArgs.Entity);
        }

        public async Task HandleAsync(EntityUpdatedEvent<Category> eventArgs)
        {
            await _mixCategoryService.UpdateMixCategoryAsync(eventArgs.Entity);
        }
    }
}
