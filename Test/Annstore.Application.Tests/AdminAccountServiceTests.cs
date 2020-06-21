using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Application.Services.Customers;
using Annstore.Auth.Entities;
using Annstore.Auth.Services;
using Annstore.Core.Common;
using Annstore.Core.Entities.Customers;
using Annstore.Services.Customers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Application.Tests
{
    public class AdminAccountServiceTests
    {
        #region Helpers
        private Mock<UserManager<Account>> GetDefaultUserManager()
        {
            var accountManagerStub = new Mock<UserManager<Account>>(Mock.Of<IUserStore<Account>>(), null, null, null, null, null, null, null, null);
            return accountManagerStub;
        }

        #endregion

        #region GetAccountListModelAsync
        [Fact]
        public async Task GetAccountListModelAsync_OptionPageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var accountListOptions = new AccountListOptions { PageNumber = pageNumber };
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentException>(() => adminAccountService.GetAccountListModelAsync(accountListOptions));
        }

        [Fact]
        public async Task GetAccountListModelAsync_OptionPageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var accountListOptions = new AccountListOptions { PageNumber = 1, PageSize = pageSize };
            var accountManagerStub = GetDefaultUserManager();
            var adminAccountService = new AdminAccountService(accountManagerStub.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentException>(() => adminAccountService.GetAccountListModelAsync(accountListOptions));
        }

        [Fact]
        public async Task GetAccountListModelAsync_ReturnAllMappedAccountSimpleModels()
        {
            var customer = Customer.CreateWithId(99);
            customer.FullName = "customer";
            var account = new Account { Id = 1, CustomerId = customer.Id };
            var mappedAccountModel = new AccountSimpleModel { Id = account.Id };
            var mappedCustomerModel = new CustomerSimpleModel { Id = customer.Id, FullName = customer.FullName };
            var accountListOptions = new AccountListOptions { PageNumber = 1, PageSize = int.MaxValue };
            var pagedAccounts = (new List<Account> { account }).ToPagedList(accountListOptions.PageSize, accountListOptions.PageNumber, 1);
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(a => a.GetPagedAccountsAsync(accountListOptions.PageNumber, accountListOptions.PageSize))
                .ReturnsAsync(pagedAccounts)
                .Verifiable();
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(customer.Id))
                .ReturnsAsync(customer)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<AccountSimpleModel>(account))
                .Returns(mappedAccountModel)
                .Verifiable();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(customer))
                .Returns(mappedCustomerModel)
                .Verifiable();
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, accountServiceMock.Object, customerServiceMock.Object, mapperMock.Object);

            var accountListModel = await adminAccountService.GetAccountListModelAsync(accountListOptions);

            var accountItem = Assert.Single(accountListModel.Accounts);
            Assert.Equal(mappedAccountModel, accountItem);
            Assert.Equal(mappedCustomerModel, accountItem.Customer);
            accountServiceMock.Verify();
            customerServiceMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task GetAccountListModelAsync_CustomerIsNull_ThrowException()
        {
            var notFoundCustomerId = 0;
            var account = new Account { Id = 1, CustomerId = notFoundCustomerId };
            var accountListOptions = new AccountListOptions { PageNumber = 1, PageSize = int.MaxValue };
            var pagedAccounts = (new List<Account> { account }).ToPagedList(accountListOptions.PageSize, accountListOptions.PageNumber, 1);
            var accountServiceStub = new Mock<IAccountService>();
            accountServiceStub.Setup(a => a.GetPagedAccountsAsync(accountListOptions.PageNumber, accountListOptions.PageSize))
                .ReturnsAsync(pagedAccounts);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(notFoundCustomerId))
                .ReturnsAsync((Customer)null)
                .Verifiable();
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, accountServiceStub.Object, customerServiceMock.Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<Exception>(() => adminAccountService.GetAccountListModelAsync(accountListOptions));

            customerServiceMock.Verify();
        }

        [Fact]
        public async Task GetAccountListModelAsync_CustomerIsDeleted_ThrowException()
        {
            var deletedCustomerId = 0;
            var deletedCustomer = Customer.CreateWithId(deletedCustomerId);
            deletedCustomer.IsDeleted(true);
            var account = new Account { Id = 1, CustomerId = deletedCustomerId };
            var accountListOptions = new AccountListOptions { PageNumber = 1, PageSize = int.MaxValue };
            var pagedAccounts = (new List<Account> { account }).ToPagedList(accountListOptions.PageSize, accountListOptions.PageNumber, 1);
            var accountServiceStub = new Mock<IAccountService>();
            accountServiceStub.Setup(a => a.GetPagedAccountsAsync(accountListOptions.PageNumber, accountListOptions.PageSize))
                .ReturnsAsync(pagedAccounts);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomerByIdAsync(deletedCustomerId))
                .ReturnsAsync(deletedCustomer)
                .Verifiable();
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, accountServiceStub.Object, customerServiceMock.Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<Exception>(() => adminAccountService.GetAccountListModelAsync(accountListOptions));

            customerServiceMock.Verify();
        }

        #endregion

        #region CreateAccountAsync
        [Fact]
        public async Task CreateAccountAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminAccountService.CreateAccountAsync(null));
        }

        [Fact]
        public async Task CreateAccountAsync_CreateAccountSuccessAndAddPasswordForAccountSuccess_SuccessResponse()
        {
            var password = "password";
            var model = new AccountModel { Id = 1, Password = password };
            var account = new Account { Id = 1 };
            var createSuccessResult = IdentityResult.Success;
            var addPasswordSuccessResult = IdentityResult.Success;
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.CreateAsync(account))
                .ReturnsAsync(createSuccessResult)
                .Verifiable();
            accountManagerMock.Setup(u => u.AddPasswordAsync(account, password))
                .ReturnsAsync(addPasswordSuccessResult)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Account>(model))
                .Returns(account)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperMock.Object);
            var request = new AppRequest<AccountModel>(model);

            var successResult = await adminAccountService.CreateAccountAsync(request);

            Assert.True(successResult.Success);
            accountManagerMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task CreateAccountAsync_AddPasswordForAccountFailed_ErrorResponse()
        {
            var password = "invalid password";
            var model = new AccountModel { Id = 1, Password = password };
            var account = new Account { Id = 1 };
            var createSuccessResult = IdentityResult.Success;
            var addPasswordFailedResult = IdentityResult.Failed();
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.CreateAsync(account))
                .ReturnsAsync(createSuccessResult)
                .Verifiable();
            accountManagerMock.Setup(u => u.AddPasswordAsync(account, password))
                .ReturnsAsync(addPasswordFailedResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<Account>(model))
                .Returns(account);
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperStub.Object);
            var request = new AppRequest<AccountModel>(model);

            var successResult = await adminAccountService.CreateAccountAsync(request);

            Assert.False(successResult.Success);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task CreateAccountAsync_CreateAccountError_ReturnErrorResponse()
        {
            var model = new AccountModel { Id = 1 };
            var account = new Account { Id = 1 };
            var createErrorResult = IdentityResult.Failed();
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.CreateAsync(account))
                .ReturnsAsync(createErrorResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<Account>(model))
                .Returns(account);
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperStub.Object);
            var request = new AppRequest<AccountModel>(model);

            var errorResult = await adminAccountService.CreateAccountAsync(request);

            Assert.False(errorResult.Success);
            accountManagerMock.Verify();
        }

        #endregion

        #region DeleteAccountAsync
        [Fact]
        public async Task DeleteAccountAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminAccountService.DeleteAccountAsync(null));
        }

        [Fact]
        public async Task DeleteAccountAsync_AccountIsNotFound_ModelInvalidResponse()
        {
            var notFoundAccountId = 0;
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.FindByIdAsync(notFoundAccountId.ToString()))
                .ReturnsAsync((Account)null)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());
            var deleteAccountRequest = new AppRequest<int>(notFoundAccountId);

            var response = await adminAccountService.DeleteAccountAsync(deleteAccountRequest);

            Assert.False(response.Success);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task DeleteAccountAsync_DeleteAccountError_ErrorResponse()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountManagerMock = GetDefaultUserManager();
            var deleteAccountErrorResult = IdentityResult.Failed();
            accountManagerMock.Setup(u => u.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            accountManagerMock.Setup(u => u.DeleteAsync(account))
                .ReturnsAsync(deleteAccountErrorResult)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());
            var deleteAccountRequest = new AppRequest<int>(accountId);

            var response = await adminAccountService.DeleteAccountAsync(deleteAccountRequest);

            Assert.False(response.Success);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task DeleteAccountAsync_DeleteAccountSuccess_SuccessResponse()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountManagerMock = GetDefaultUserManager();
            var deleteAccountSuccessResult = IdentityResult.Success;
            accountManagerMock.Setup(u => u.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            accountManagerMock.Setup(u => u.DeleteAsync(account))
                .ReturnsAsync(deleteAccountSuccessResult)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());
            var deleteAccountRequest = new AppRequest<int>(accountId);

            var response = await adminAccountService.DeleteAccountAsync(deleteAccountRequest);

            Assert.True(response.Success);
            accountManagerMock.Verify();
        }

        #endregion

        #region GetAccountModelAsync
        [Fact]
        public async Task GetAccountModelAsync_AccountIsNotFound_ReturnNullAccountModel()
        {
            var notFoundAccountId = 0;
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.FindByIdAsync(notFoundAccountId.ToString()))
                .ReturnsAsync((Account)null)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            var nullAccountModel = await adminAccountService.GetAccountModelAsync(notFoundAccountId);

            Assert.IsType<NullAccountModel>(nullAccountModel);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task GetAccountModelAsync_AccountIsFound_ReturnMappedModel()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountModel = new AccountModel { Id = accountId };
            var allCustomers = new List<Customer> { Customer.CreateWithId(1), Customer.CreateWithId(2) };
            var mappedCustomers = new List<CustomerSimpleModel> { new CustomerSimpleModel { Id = 1 }, new CustomerSimpleModel { Id = 2 } };
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomersAsync())
                .ReturnsAsync(allCustomers)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<AccountModel>(account))
                .Returns(accountModel)
                .Verifiable();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(allCustomers[0]))
                .Returns(mappedCustomers[0])
                .Verifiable();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(allCustomers[1]))
                .Returns(mappedCustomers[1])
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), customerServiceMock.Object, mapperMock.Object);

            var result = await adminAccountService.GetAccountModelAsync(accountId);

            Assert.Equal(accountModel, result);
            Assert.Equal(allCustomers.Count, result.Customers.Count);
            Assert.Equal(allCustomers[0].Id, result.Customers[0].Id);
            Assert.Equal(allCustomers[1].Id, result.Customers[1].Id);
            mapperMock.Verify();
        }

        #endregion

        #region UpdateAccountAsync
        [Fact]
        public async Task UpdateAccountAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminAccountService.UpdateAccountAsync(null));
        }

        [Fact]
        public async Task UpdateAccountAsync_AccountIsNotFound_ModelInvalidResponse()
        {
            var notFoundAccountId = 0;
            var accountModel = new AccountModel { Id = notFoundAccountId };
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(u => u.FindByIdAsync(notFoundAccountId.ToString()))
                .ReturnsAsync((Account)null)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());
            var updateAccountRequest = new AppRequest<AccountModel>(accountModel);

            var response = await adminAccountService.UpdateAccountAsync(updateAccountRequest);

            Assert.False(response.Success);
            Assert.True(response.ModelIsInvalid);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task UpdateAccountAsync_UpdateAccountSuccess_SuccessResponse()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountModel = new AccountModel { Id = accountId };
            var mappedAccount = new Account { Id = accountId };
            var updateAccountResult = IdentityResult.Success;
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(m => m.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            accountManagerMock.Setup(m => m.UpdateAsync(mappedAccount))
                .ReturnsAsync(updateAccountResult)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(accountModel, account))
                .Returns(mappedAccount)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperMock.Object);
            var updateAccountRequest = new AppRequest<AccountModel>(accountModel);

            var response = await adminAccountService.UpdateAccountAsync(updateAccountRequest);

            Assert.True(response.Success);
            accountManagerMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task UpdateAccountAsync_RemovePasswordError_ErrorResponse()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountModel = new AccountModel { Id = accountId, Password = "change password" };
            var mappedAccount = new Account { Id = accountId };
            var updateAccountResult = IdentityResult.Success;
            var removePasswordResult = IdentityResult.Failed();
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(m => m.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            accountManagerMock.Setup(m => m.UpdateAsync(mappedAccount))
                .ReturnsAsync(updateAccountResult);
            accountManagerMock.Setup(m => m.RemovePasswordAsync(mappedAccount))
                .ReturnsAsync(removePasswordResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map(accountModel, account))
                .Returns(mappedAccount)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperStub.Object);
            var updateAccountRequest = new AppRequest<AccountModel>(accountModel);

            var response = await adminAccountService.UpdateAccountAsync(updateAccountRequest);

            Assert.False(response.Success);
            accountManagerMock.Verify();
        }

        [Fact]
        public async Task UpdateAccountAsync_AddPasswordError_ErrorResponse()
        {
            var accountId = 1;
            var account = new Account { Id = accountId };
            var accountModel = new AccountModel { Id = accountId, Password = "change password" };
            var mappedAccount = new Account { Id = accountId };
            var updateAccountResult = IdentityResult.Success;
            var removePasswordResult = IdentityResult.Success;
            var addPasswordResult = IdentityResult.Failed();
            var accountManagerMock = GetDefaultUserManager();
            accountManagerMock.Setup(m => m.FindByIdAsync(accountId.ToString()))
                .ReturnsAsync(account);
            accountManagerMock.Setup(m => m.UpdateAsync(mappedAccount))
                .ReturnsAsync(updateAccountResult);
            accountManagerMock.Setup(m => m.RemovePasswordAsync(mappedAccount))
                .ReturnsAsync(removePasswordResult);
            accountManagerMock.Setup(m => m.AddPasswordAsync(mappedAccount, accountModel.Password))
                .ReturnsAsync(addPasswordResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map(accountModel, account))
                .Returns(mappedAccount)
                .Verifiable();
            var adminAccountService = new AdminAccountService(accountManagerMock.Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), mapperStub.Object);
            var updateAccountRequest = new AppRequest<AccountModel>(accountModel);

            var response = await adminAccountService.UpdateAccountAsync(updateAccountRequest);

            Assert.False(response.Success);
            accountManagerMock.Verify();
        }

        #endregion

        #region PrepareCustomersForAccountAsync
        [Fact]
        public async Task PrepareCustomersForAccountAsync_ModelIsNull_ThrowArgumentNullException()
        {
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), Mock.Of<ICustomerService>(), Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminAccountService.PrepareCustomersForAccountAsync(null));
        }

        [Fact]
        public async Task PrepareCustomersForAccountAsync_ModelIsNotNull_IncludeMappedAllCustomers()
        {
            var model = new AccountModel();
            var availableCustomers = new List<Customer> { Customer.CreateWithId(1), Customer.CreateWithId(2) };
            var mappedCustomers = new List<CustomerSimpleModel> { new CustomerSimpleModel { Id = 1 }, new CustomerSimpleModel { Id = 2 } };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.GetCustomersAsync())
                .ReturnsAsync(availableCustomers)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(availableCustomers[0]))
                .Returns(mappedCustomers[0])
                .Verifiable();
            mapperMock.Setup(m => m.Map<CustomerSimpleModel>(availableCustomers[1]))
                .Returns(mappedCustomers[1])
                .Verifiable();
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), customerServiceMock.Object, mapperMock.Object);

            var result = await adminAccountService.PrepareCustomersForAccountAsync(model);

            Assert.Equal(availableCustomers.Count, result.Customers.Count);
            Assert.Equal(availableCustomers[0].Id, result.Customers[0].Id);
            Assert.Equal(availableCustomers[1].Id, result.Customers[1].Id);
            customerServiceMock.Verify();
            mapperMock.Verify();
        }

        #endregion

        #region HasCustomersAsync
        [Fact]
        public async Task HasCustomersAsync()
        {
            var availableCustomers = new List<Customer> { Customer.CreateWithId(1) };
            var customerServiceMock = new Mock<ICustomerService>();
            customerServiceMock.Setup(c => c.HasCustomersAsync())
                .ReturnsAsync(availableCustomers.Count > 0)
                .Verifiable();
            var adminAccountService = new AdminAccountService(GetDefaultUserManager().Object, Mock.Of<IAccountService>(), customerServiceMock.Object, Mock.Of<IMapper>());

            var result = await adminAccountService.HasCustomersAsync();

            Assert.Equal(availableCustomers.Count > 0, result);
            customerServiceMock.Verify();
        }

        #endregion
    }
}
