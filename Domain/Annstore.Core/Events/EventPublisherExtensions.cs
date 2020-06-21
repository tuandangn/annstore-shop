using System.Threading.Tasks;

namespace Annstore.Core.Events
{
    public static class EventPublisherExtensions
    {
        public static Task EntityCreated<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : class
        {
            var entityCreatedEvent = new EntityCreatedEvent<TEntity>(entity);
            return eventPublisher.PublishAsync(entityCreatedEvent);
        }

        public static Task EntityUpdated<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : class
        {
            var entityUpdatedEvent = new EntityUpdatedEvent<TEntity>(entity);
            return eventPublisher.PublishAsync(entityUpdatedEvent);
        }

        public static Task EntityDeleted<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : class
        {
            var entityDeletedEvent = new EntityDeletedEvent<TEntity>(entity);
            return eventPublisher.PublishAsync(entityDeletedEvent);
        }
    }
}
