using Annstore.Core.Common;
using Annstore.Core.Entities.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Services.Customers
{
    public interface ICustomerService
    {
        Task<bool> HasCustomersAsync();

        Task<Customer> GetCustomerByIdAsync(int id);

        Task<List<Customer>> GetCustomersAsync();

        Task<IPagedList<Customer>> GetPagedCustomersAsync(int pageNumber, int pageSize);

        Task<Customer> CreateCustomerAsync(Customer customer);

        Task<Customer> UpdateCustomerAsync(Customer customer);

        Task DeleteCustomerAsync(Customer customer);
    }
}
