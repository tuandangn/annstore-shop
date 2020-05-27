using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Services.Customers;
using Annstore.Auth.Entities;
using Annstore.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.Tests.Admin
{
    public class AccountControllerTests
    {
        #region Helper
        private IOptionsSnapshot<AccountSettings> GetDefaultAccountOptions(int defaultPageSize = int.MaxValue)
        {
            var accountSettings = new AccountSettings
            {
                Admin = new AccountSettings.AdminAccountSettings { DefaultPageSize = defaultPageSize }
            };
            var accountSettingsSnapshotStub = new Mock<IOptionsSnapshot<AccountSettings>>();
            accountSettingsSnapshotStub.Setup(u => u.Value)
                .Returns(accountSettings);

            return accountSettingsSnapshotStub.Object;
        }

        #endregion

        #region Index
        [Fact]
        public void Index_RedirectToList()
        {
            var accountController = new AccountController(Mock.Of<IAdminAccountService>(), GetDefaultAccountOptions());

            var result = accountController.Index();

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToList.ActionName);
        }

        #endregion

        #region List
        [Fact]
        public async Task List_ReturnViewWithValidModel()
        {
            var accountListModel = new AccountListModel();
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.GetAccountListModelAsync(It.IsAny<AccountListOptions>()))
                .ReturnsAsync(accountListModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());

            var result = await accountController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(accountListModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task List_PaginationInfoInvalid_UseDefaultValues()
        {
            var defaultPageSize = 12;
            var page = 0;
            var size = 0;
            var accountListModel = new AccountListModel();
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.GetAccountListModelAsync(
                It.Is<AccountListOptions>(opts =>
                    opts.PageNumber == 1 && opts.PageSize == defaultPageSize
                )))
                .ReturnsAsync(accountListModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions(defaultPageSize));

            var result = await accountController.List(page, size);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(accountListModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        #endregion

        #region Create
        [Fact]
        public async Task Create_HasCustomersIsFalse_RedirectToList()
        {
            var hasCustomers = false;
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.HasCustomersAsync())
                .ReturnsAsync(hasCustomers)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Create();

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToAction.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task Create_HasCustomerIsTrue_ReturnValidModel()
        {
            var hasCustomers = true;
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.HasCustomersAsync())
                .ReturnsAsync(hasCustomers);
            adminAccountServiceMock.Setup(c => c.PrepareCustomersForAccountAsync(It.IsAny<AccountModel>()))
                .ReturnsAsync(It.IsNotIn<AccountModel>())
                .Verifiable() ;
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());

            var result = await accountController.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        #endregion

        #region Create Post
        [Fact]
        public async Task CreatePost_ModelIsInvalid_RedisplayViewWithValidModel()
        {
            var invalidAccountModel = new AccountModel();
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.PrepareCustomersForAccountAsync(invalidAccountModel))
                .ReturnsAsync(invalidAccountModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.ModelState.AddModelError("error", "error");

            var result = await accountController.Create(invalidAccountModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidAccountModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_CreateAccountSuccess_RedirectToList()
        {
            var accountModel = new AccountModel();
            var account = new Account();
            var createSuccessResponse = AppResponse.SuccessResult(account);
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(u => u.CreateAccountAsync(It.Is<AppRequest<AccountModel>>(req => req.Data == accountModel)))
                .ReturnsAsync(createSuccessResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Create(accountModel);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToList.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_CreateAccountError_RedisplayView()
        {
            var accountModel = new AccountModel();
            var account = new Account();
            var createErrorResponse = AppResponse.ErrorResult<Account>(string.Empty);
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(u => u.CreateAccountAsync(It.Is<AppRequest<AccountModel>>(req => req.Data == accountModel)))
                .ReturnsAsync(createErrorResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Create(accountModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(accountModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        #endregion

        #region Delete Post
        [Fact]
        public async Task DeletePost_AccountIsNotFound_RedirectToList()
        {
            var notFoundAccountId = 0;
            var deleteAccountModelInvalidResponse = AppResponse.InvalidModelResult("model invalid");
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.DeleteAccountAsync(It.Is<AppRequest<int>>(req => req.Data == notFoundAccountId)))
                .ReturnsAsync(deleteAccountModelInvalidResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Delete(notFoundAccountId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToList.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task DeletePost_DeleteAccountError_RedirectToList()
        {
            var deleteAccountId = 1;
            var deleteAccountErrorResponse = AppResponse.ErrorResult("delete account error");
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.DeleteAccountAsync(It.Is<AppRequest<int>>(req => req.Data == deleteAccountId)))
                .ReturnsAsync(deleteAccountErrorResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Delete(deleteAccountId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToList.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task DeletePost_DeleteAccountSuccc_RedirectToList()
        {
            var deleteAccountId = 1;
            var deleteAccountSuccessResponse = AppResponse.ErrorResult("delete account error");
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.DeleteAccountAsync(It.Is<AppRequest<int>>(req => req.Data == deleteAccountId)))
                .ReturnsAsync(deleteAccountSuccessResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Delete(deleteAccountId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToList.ActionName);
            adminAccountServiceMock.Verify();
        }

        #endregion

        #region Edit
        [Fact]
        public async Task Edit_HasCustomersIsFalse_RedirectToList()
        {
            var hasCustomers = false;
            var accountId = 0;
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.HasCustomersAsync())
                .ReturnsAsync(hasCustomers)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Edit(accountId);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToAction.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_ModelIsNullAccountModel_RedirectToList()
        {
            var hasCustomers = true;
            var accountId = 0;
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.HasCustomersAsync())
                .ReturnsAsync(hasCustomers);
            adminAccountServiceMock.Setup(a => a.GetAccountModelAsync(accountId))
                .ReturnsAsync(AccountModel.NullModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());

            var result = await accountController.Edit(accountId);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToAction.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_AccountModelIsNotNull_ReturnView()
        {
            var accountId = 1;
            var accountModel = new AccountModel { Id = accountId };
            var hasCustomers = true;
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.HasCustomersAsync())
                .ReturnsAsync(hasCustomers);
            adminAccountServiceMock.Setup(a => a.GetAccountModelAsync(accountId))
                .ReturnsAsync(accountModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());

            var result = await accountController.Edit(accountId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(accountModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        #endregion

        #region Edit Post
        [Fact]
        public async Task EditPost_ModelStateIsInvalid_RedisplayView()
        {
            var accountModel = new AccountModel();
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.PrepareCustomersForAccountAsync(accountModel))
                .ReturnsAsync(accountModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.ModelState.AddModelError("error", "error");

            var result = await accountController.Edit(accountModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(accountModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_ModelInvalidResponse_RedirectToList()
        {
            var accountModel = new AccountModel();
            var modelInvalidResponse = AppResponse.InvalidModelResult<Account>("model invalid");
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.UpdateAccountAsync(It.Is<AppRequest<AccountModel>>(req => req.Data == accountModel)))
                .ReturnsAsync(modelInvalidResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Edit(accountModel);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToAction.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_UpdateAccountIsSuccess_RedirectToList()
        {
            var accountModel = new AccountModel();
            var updateAccountSuccessResponse = AppResponse.SuccessResult(new Account());
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.UpdateAccountAsync(It.Is<AppRequest<AccountModel>>(req => req.Data == accountModel)))
                .ReturnsAsync(updateAccountSuccessResponse)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Edit(accountModel);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.List), redirectToAction.ActionName);
            adminAccountServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_UpdateAccountError_RedisplayView()
        {
            var accountModel = new AccountModel();
            var updateAccountErrorResponse = AppResponse.ErrorResult<Account>("update error");
            var adminAccountServiceMock = new Mock<IAdminAccountService>();
            adminAccountServiceMock.Setup(a => a.UpdateAccountAsync(It.Is<AppRequest<AccountModel>>(req => req.Data == accountModel)))
                .ReturnsAsync(updateAccountErrorResponse)
                .Verifiable();
            adminAccountServiceMock.Setup(a => a.PrepareCustomersForAccountAsync(accountModel))
                .ReturnsAsync(accountModel)
                .Verifiable();
            var accountController = new AccountController(adminAccountServiceMock.Object, GetDefaultAccountOptions());
            accountController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await accountController.Edit(accountModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(accountModel, viewResult.Model);
            adminAccountServiceMock.Verify();
        }

        #endregion
    }
}
