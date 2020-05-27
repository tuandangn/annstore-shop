using Annstore.Auth.Entities;
using Annstore.Core.Entities.Customers;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Annstore.Core.Common;

namespace Annstore.Auth.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly UserManager<Account> _userManager;

        public AccountService(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            var query = from account in _userManager.Users
                        select account;
            var result = await query.ToListAsync()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<List<Account>> GetAccountsOfCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var query = from account in _userManager.Users
                        where account.CustomerId == customer.Id
                        orderby account.Id
                        select account;

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<IPagedList<Account>> GetPagedAccountsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must greater than or equal 1");
            if (pageSize <= 0)
                throw new ArgumentException("Page size must greater than 0");

            var query = from account in _userManager.Users
                        orderby account.Id descending
                        select account;
            //*TODO*
            var allAccounts = await query.ToListAsync()
                .ConfigureAwait(false);
            var accounts = allAccounts.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var result = accounts.ToPagedList(pageSize, pageNumber, allAccounts.Count);

            return result;
        }

    }
}
