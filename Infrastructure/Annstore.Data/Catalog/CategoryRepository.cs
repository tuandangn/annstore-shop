using Annstore.Core.Entities.Catalog;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Data.Catalog
{
    public sealed class CategoryRepository : RepositoryBase<Category>
    {
        private readonly IEventPublisher _eventPublisher;

        public CategoryRepository(IDbContext dbContext, IEventPublisher eventPublisher) : base(dbContext)
        {
            _eventPublisher = eventPublisher;
        }

        public override async ValueTask<Category> InsertAsync(Category entity)
        {
            var insertResult = await base.InsertAsync(entity);

            await _eventPublisher.EntityCreated(entity);
            return insertResult;
        }

        public override async ValueTask<Category> UpdateAsync(Category entity)
        {
            var updateResult = await base.UpdateAsync(entity);

            await _eventPublisher.EntityUpdated(entity);
            return updateResult;
        }

        public override async Task DeleteAsync(Category entity)
        {
            await base.DeleteAsync(entity);
            await _eventPublisher.EntityDeleted(entity);
        }
    }
}
