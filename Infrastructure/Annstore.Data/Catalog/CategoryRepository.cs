using Annstore.Core.Entities.Catalog;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Data.Catalog
{
    public sealed class CategoryRepository : PublishEventRepository<Category>, ICategoryRepository
    {
        private readonly IEventPublisher _eventPublisher;

        public CategoryRepository(IDbContext dbContext, IEventPublisher eventPublisher) : base(dbContext, eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public override async Task DeleteAsync(Category entity)
        {
            entity.IsDeleted(true);

            await UpdateAsync(entity)
                .ConfigureAwait(false);

            await _eventPublisher.EntityDeleted(entity)
                .ConfigureAwait(false);
        }
    }
}
