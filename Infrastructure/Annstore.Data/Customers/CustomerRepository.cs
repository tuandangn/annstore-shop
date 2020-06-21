using Annstore.Core.Entities.Customers;
using Annstore.Core.Events;
using System.Threading.Tasks;

namespace Annstore.Data.Customers
{
    public sealed class CustomerRepository : PublishEventRepository<Customer>, ICustomerRepository
    {
        private readonly IEventPublisher _eventPublisher;

        public CustomerRepository(IDbContext dbContext, IEventPublisher eventPublisher) : base(dbContext, eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public override async Task DeleteAsync(Customer entity)
        {
            entity.IsDeleted(true);
            await UpdateAsync(entity);

            await _eventPublisher.EntityDeleted(entity);
        }
    }
}
