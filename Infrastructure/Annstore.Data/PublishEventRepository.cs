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
            var insertResult = await base.InsertAsync(entity);

            await _eventPublisher.EntityCreated(entity);
            return insertResult;
        }

        public override async ValueTask<TEntity> UpdateAsync(TEntity entity)
        {
            var updateResult = await base.UpdateAsync(entity);

            await _eventPublisher.EntityUpdated(entity);
            return updateResult;
        }

        public override async Task DeleteAsync(TEntity entity)
        {
            await base.DeleteAsync(entity);
            await _eventPublisher.EntityDeleted(entity);
        }
    }
}
