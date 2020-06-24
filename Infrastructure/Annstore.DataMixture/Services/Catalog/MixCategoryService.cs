using Annstore.Core.Events;
using System;
using System.Threading.Tasks;
using MixCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Services.Catalog
{
    public class MixCategoryService : IMixCategoryService
    {
        private readonly IMixRepository<MixCategory> _mixCategoryRepository;
        private readonly IEventPublisher _eventPublisher;

        public MixCategoryService(IMixRepository<MixCategory> mixCategoryRepository, IEventPublisher eventPublisher)
        {
            _mixCategoryRepository = mixCategoryRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<MixCategory> GetMixCategoryByEntityIdAsync(int id)
        {
            return await _mixCategoryRepository.FindByEntityIdAsync(id)
                .ConfigureAwait(false);
        }

        public async Task<MixCategory> CreateMixCategoryAsync(MixCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category = await _mixCategoryRepository.InsertAsync(category)
                .ConfigureAwait(false);

            await _eventPublisher.EntityCreated(category)
                .ConfigureAwait(false);
            return category;
        }

        public async Task DeleteMixCategoryAsync(MixCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await _mixCategoryRepository.DeleteAsync(category)
                .ConfigureAwait(false);

            await _eventPublisher.EntityDeleted(category)
                .ConfigureAwait(false);
        }

        public async Task<MixCategory> UpdateMixCategoryAsync(MixCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category = await _mixCategoryRepository.UpdateAsync(category)
                .ConfigureAwait(false);

            await _eventPublisher.EntityUpdated(category)
                .ConfigureAwait(false);
            return category;
        }

    }
}
