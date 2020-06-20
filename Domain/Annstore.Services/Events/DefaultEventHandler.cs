using Annstore.Core.Entities.Catalog;
using Annstore.Core.Entities.Customers;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Services.Events
{
    public class DefaultEventHandler :
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
        public Task Handle(EntityCreatedEvent<Category> targetEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(EntityDeletedEvent<Category> targetEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(EntityUpdatedEvent<Category> targetEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(EntityCreatedEvent<Customer> targetEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(EntityDeletedEvent<Customer> targetEvent)
        {
            throw new System.NotImplementedException();
        }


        public Task Handle(EntityUpdatedEvent<Customer> targetEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}
