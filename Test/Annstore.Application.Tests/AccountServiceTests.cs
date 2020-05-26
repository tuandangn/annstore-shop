using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Application.Services.Customers;
using Annstore.Auth.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Application.Tests
{
    public class AccountServiceTests
    {
        #region Helpers
        private Mock<UserManager<Account>> GetDefaultUserManager()
        {
            var userManagerStub = new Mock<UserManager<Account>>(Mock.Of<IUserStore<Account>>(), null, null, null, null, null, null, null, null);
            return userManagerStub;
        }

        private Mock<SignInManager<Account>> GetDefaultSignInManager(UserManager<Account> userManager)
        {
            var signInManagerStub = new Mock<SignInManager<Account>>(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<Account>>(), null, null, null);
            return signInManagerStub;
        }

        #endregion

        #region LoginAsync
        [Fact]
        public async Task LoginAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var userManagerStub = GetDefaultUserManager();
            var signInManagerStub = GetDefaultSignInManager(userManagerStub.Object);
            var accountService = new AccountService(userManagerStub.Object, signInManagerStub.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => accountService.LoginAsync(null));
        }

        [Fact]
        public async Task LoginAsync_UserIsNotFound_ErrorResponseWithMessage()
        {
            var notFoundEmail = "not found email";
            var model = new LoginModel
            {
                Email = notFoundEmail
            };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByEmailAsync(notFoundEmail))
                .ReturnsAsync((Account)null)
                .Verifiable();
            var signInManagerStub = GetDefaultSignInManager(userManagerMock.Object);
            var accountService = new AccountService(userManagerMock.Object, signInManagerStub.Object);
            var request = new AppRequest<LoginModel>(model);

            var response = await accountService.LoginAsync(request);

            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task LoginAsync_PasswordIsInvalid_ErrorResponseWithMessage()
        {
            var email = "email";
            var invalidPassword = "invalid password";
            var model = new LoginModel { Email = email, Password = invalidPassword };
            var account = new Account { Email = email };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByEmailAsync(email))
                .ReturnsAsync(account);
            userManagerMock.Setup(u => u.CheckPasswordAsync(account, invalidPassword))
                .ReturnsAsync(false)
                .Verifiable();
            var signInManagerStub = GetDefaultSignInManager(userManagerMock.Object);
            var accountService = new AccountService(userManagerMock.Object, signInManagerStub.Object);
            var request = new AppRequest<LoginModel>(model);

            var response = await accountService.LoginAsync(request);

            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task LoginAsync_SignInSuccess_SignInUser()
        {
            var email = "email";
            var password = "password";
            var model = new LoginModel { Email = email, Password = password, Remember = true };
            var account = new Account { Email = email };
            var successSignInResult = SignInResult.Success;
            var userManagerStub = GetDefaultUserManager();
            userManagerStub.Setup(u => u.FindByEmailAsync(email))
                .ReturnsAsync(account);
            userManagerStub.Setup(u => u.CheckPasswordAsync(account, password))
                .ReturnsAsync(true);
            var signInManagerMock = GetDefaultSignInManager(userManagerStub.Object);
            signInManagerMock.Setup(s => s.PasswordSignInAsync(account, password, model.Remember, false))
                .ReturnsAsync(successSignInResult)
                .Verifiable();
            var accountService = new AccountService(userManagerStub.Object, signInManagerMock.Object);
            var request = new AppRequest<LoginModel>(model);

            var response = await accountService.LoginAsync(request);

            Assert.True(response.Success);
            Assert.Equal(account, response.Result);
            signInManagerMock.Verify();
        }

        [Fact]
        public async Task LoginAsync_SignInFailed_ErrorResponseWithMessage()
        {
            var email = "email";
            var password = "password";
            var model = new LoginModel { Email = email, Password = password, Remember = true };
            var account = new Account { Email = email };
            var failedSignInResult = SignInResult.Failed;
            var userManagerStub = GetDefaultUserManager();
            userManagerStub.Setup(u => u.FindByEmailAsync(email))
                .ReturnsAsync(account);
            userManagerStub.Setup(u => u.CheckPasswordAsync(account, password))
                .ReturnsAsync(true);
            var signInManagerMock = GetDefaultSignInManager(userManagerStub.Object);
            signInManagerMock.Setup(s => s.PasswordSignInAsync(account, password, model.Remember, false))
                .ReturnsAsync(failedSignInResult)
                .Verifiable();
            var accountService = new AccountService(userManagerStub.Object, signInManagerMock.Object);
            var request = new AppRequest<LoginModel>(model);

            var response = await accountService.LoginAsync(request);

            Assert.False(response.Success);
            Assert.NotNull(response.Message);
            signInManagerMock.Verify();
        }

        #endregion
    }

}
