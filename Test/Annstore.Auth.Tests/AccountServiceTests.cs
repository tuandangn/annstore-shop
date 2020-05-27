using Annstore.Auth.Entities;
using Annstore.Auth.Services;
using Annstore.Core.Entities.Customers;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using TestHelper;
using System.Linq;

namespace Annstore.Auth.Tests
{
    public class AccountServiceTests
    {
        #region Helpers
        private Mock<UserManager<Account>> GetDefaultUserManager()
        {
            var accountManagerStub = new Mock<UserManager<Account>>(Mock.Of<IUserStore<Account>>(), null, null, null, null, null, null, null, null);
            return accountManagerStub;
        }

        #endregion

        #region GetAccountsOfCustomerAsync
        [Fact]
        public async Task GetAccountsOfCustomerAsync_CustomerIsNull_ThrowArgumentNullException()
        {
            var accountService = new AccountService(GetDefaultUserManager().Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => accountService.GetAccountsOfCustomerAsync(null));
        }

        [Fact]
        public async Task GetAccountsOfCustomerAsync_CustomerNotNull_ReturnAccountsOfCustomer()
        {
            var customer = new Customer { Id = 1 };
            var availableAccounts = new List<Account> { new Account { Id = 1, CustomerId = 1 }, new Account { Id = 2, CustomerId = 1 } };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(availableAccounts.ToAsync())
                .Verifiable();
            var accountService = new AccountService(userManagerMock.Object);

            var result = await accountService.GetAccountsOfCustomerAsync(customer);

            Assert.Equal(2, result.Count);
            Assert.Equal(availableAccounts[0].Id, result[0].Id);
            Assert.Equal(availableAccounts[1].Id, result[1].Id);
            userManagerMock.Verify();

            await Assert.ThrowsAsync<ArgumentNullException>(() => accountService.GetAccountsOfCustomerAsync(null));
        }

        #endregion

        #region GetAccountsAsync
        [Fact]
        public async Task GetAccountsAsync_ReturnAllAccounts()
        {
            var availableAccounts = new List<Account> { new Account { Id = 1 }, new Account { Id = 2 } };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(availableAccounts.ToAsync())
                .Verifiable();
            var accountService = new AccountService(userManagerMock.Object);

            var result = await accountService.GetAccountsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(availableAccounts[0], result[0]);
            Assert.Equal(availableAccounts[1], result[1]);
            userManagerMock.Verify();
        }

        #endregion

        #region GetPagedAccountsAsync
        [Fact]
        public async Task GetPagedAccountsAsync_PageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var accountService = new AccountService(GetDefaultUserManager().Object);

            await Assert.ThrowsAsync<ArgumentException>(() => accountService.GetPagedAccountsAsync(pageNumber, int.MaxValue));
        }

        [Fact]
        public async Task GetPagedAccountsAsync_PageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var accountService = new AccountService(GetDefaultUserManager().Object);

            await Assert.ThrowsAsync<ArgumentException>(() => accountService.GetPagedAccountsAsync(1, pageSize));
        }

        [Fact]
        public async Task GetPagedAccountsAsync_ReturnValidResult()
        {
            var pageNumber = 2;
            var pageSize = 2;
            var availableAccounts = new List<Account>
            {
                new Account{Id = 1 }, new Account{Id = 2 },
                new Account{Id = 3 }, new Account{Id = 4 },
                new Account{Id = 5 }, new Account{Id = 6 }
            };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(c => c.Users)
                .Returns(availableAccounts.ToAsync())
                .Verifiable();
            var accountService = new AccountService(userManagerMock.Object);

            var result = await accountService.GetPagedAccountsAsync(pageNumber, pageSize);

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(availableAccounts.Count, result.TotalItems);
            //order by id desc
            Assert.Equal(4, result.Items.ElementAt(0).Id);
            userManagerMock.Verify();
        }
        #endregion
    }
}
