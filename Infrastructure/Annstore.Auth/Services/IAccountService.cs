using Annstore.Auth.Entities;
using Annstore.Core.Common;
using Annstore.Core.Entities.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Auth.Services
{
    public interface IAccountService
    {
        Task<List<Account>> GetAccountsAsync();

        Task<IPagedList<Account>> GetPagedAccountsAsync(int pageNumber, int pageSize);

        Task<List<Account>> GetAccountsOfCustomerAsync(Customer customer);
    }
}
