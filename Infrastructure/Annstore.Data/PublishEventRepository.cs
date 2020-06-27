using Annstore.Core;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Data
{
    public class PublishEventRepository<TEntity> : RepositoryBase<TEntity> where TEntity : class, IAggregateRoot
    {
        private readonly IEventPublisher _eventPublisher;

        public PublishEventRepository(IDbContext dbContext, IEventPublisher eventPublisher) : base(dbContext)
        {
            _eventPublisher = eventPublisher;
        }

        public override async ValueTask<TEntity> InsertAsync(TEntity entity)
        {
            var insertResult = await base.InsertAsync(entity)
                .ConfigureAwait(false);

            await _eventPublisher.EntityCreated(entity)
                .ConfigureAwait(false);
            return insertResult;
        }

        public override async ValueTask<TEntity> UpdateAsync(TEntity entity)
        {
            var updateResult = await base.UpdateAsync(entity)
                .ConfigureAwait(false);

            await _eventPublisher.EntityUpdated(entity)
                .ConfigureAwait(false);
            return updateResult;
        }

        public override async Task DeleteAsync(TEntity entity)
        {
            await base.DeleteAsync(entity)
                .ConfigureAwait(false);
            await _eventPublisher.EntityDeleted(entity)
                .ConfigureAwait(false);
        }
    }
}
