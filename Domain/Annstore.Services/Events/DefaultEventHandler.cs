using Annstore.Core.Entities.Catalog;
using Annstore.Core.Entities.Customers;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Services.Events
{
    public sealed class DefaultEventHandler :
        IEventHandler<EntityCreatedEvent<Category>>,
        IEventHandler<EntityDeletedEvent<Category>>,
        IEventHandler<EntityUpdatedEvent<Category>>,

        IEventHandler<EntityCreatedEvent<Customer>>,
        IEventHandler<EntityDeletedEvent<Customer>>,
        IEventHandler<EntityUpdatedEvent<Customer>>
    {
        public DefaultEventHandler()
        {

        }

        public Task HandleAsync(EntityCreatedEvent<Category> targetEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityDeletedEvent<Category> targetEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Category> targetEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityCreatedEvent<Customer> targetEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityDeletedEvent<Customer> targetEvent)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdatedEvent<Customer> targetEvent)
        {
            return Task.CompletedTask;
        }
    }
}
