using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Application.Services.Customers;
using Annstore.Auth.Entities;
using Annstore.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.Tests
{
    public class AccountControllerTests
    {
        #region Login Post
        [Fact]
        public async Task LoginPost_ModelIsInvalid_RedisplayView()
        {
            var accountController = new AccountController(Mock.Of<IPublicAccountService>());
            accountController.ModelState.AddModelError("error", "error");

            var result = await accountController.SignIn(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task LoginPost_LoginSuccess_RedirectToHomepage()
        {
            var loginModel = new LoginModel();
            var successLoginResponse = AppResponse.SuccessResult<Account>(null);
            var accountServiceMock = new Mock<IPublicAccountService>();
            accountServiceMock.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(successLoginResponse)
                .Verifiable();
            var accountController = new AccountController(accountServiceMock.Object);

            var result = await accountController.SignIn(loginModel, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName, true);
            accountServiceMock.Verify();
        }

        [Fact]
        public async Task LoginPost_ReturnUrlNotNull_LocalRedirectToReturnUrl()
        {
            var returnUrl = "returnUrl";
            var loginModel = new LoginModel();
            var successLoginResponse = AppResponse.SuccessResult<Account>(null);
            var accountServiceStub = new Mock<IPublicAccountService>();
            accountServiceStub.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(successLoginResponse);
            var accountController = new AccountController(accountServiceStub.Object);

            var result = await accountController.SignIn(loginModel, returnUrl);

            var localRedirectResult = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, localRedirectResult.Url);
        }

        [Fact]
        public async Task LoginPost_LoginFailed_RedirectView()
        {
            var loginModel = new LoginModel();
            var failedLoginResponse = AppResponse.ErrorResult<Account>(string.Empty);
            var accountServiceMock = new Mock<IPublicAccountService>();
            accountServiceMock.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(failedLoginResponse)
                .Verifiable();
            var accountController = new AccountController(accountServiceMock.Object);

            var result = await accountController.SignIn(loginModel, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            accountServiceMock.Verify();
        }

        #endregion

        #region Logout Post
        [Fact]
        public async Task LogoutPost_RedirectToHomepage()
        {
            var publicAccountServiceMock = new Mock<IPublicAccountService>();
            publicAccountServiceMock.Setup(a => a.LogoutAsync(It.IsAny<ClaimsPrincipal>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var accountController = new AccountController(publicAccountServiceMock.Object);

            var result = await accountController.SignOut();

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(HomeController.Index), redirectToAction.ActionName);
            Assert.Equal("Home", redirectToAction.ControllerName, true);
            publicAccountServiceMock.Verify();
        }

        #endregion
    }
}
