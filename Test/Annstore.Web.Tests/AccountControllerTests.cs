using Annstore.Core.Entities.Users;
using Annstore.Web.Controllers;
using Annstore.Web.Infrastructure;
using Annstore.Web.Models.Accounts;
using Annstore.Web.Services.Accounts;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
            var accountController = new AccountController(Mock.Of<IAccountService>());
            accountController.ModelState.AddModelError("error", "error");

            var result = await accountController.Login(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task LoginPost_LoginSuccess_RedirectToHomepage()
        {
            var loginModel = new LoginModel();
            var successLoginResponse = AppResponse.SuccessResult<AppUser>(null);
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(successLoginResponse)
                .Verifiable();
            var accountController = new AccountController(accountServiceMock.Object);

            var result = await accountController.Login(loginModel, null);

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
            var successLoginResponse = AppResponse.SuccessResult<AppUser>(null);
            var accountServiceStub = new Mock<IAccountService>();
            accountServiceStub.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(successLoginResponse);
            var accountController = new AccountController(accountServiceStub.Object);

            var result = await accountController.Login(loginModel, returnUrl);

            var localRedirectResult = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, localRedirectResult.Url);
        }

        [Fact]
        public async Task LoginPost_LoginFailed_RedirectView()
        {
            var loginModel = new LoginModel();
            var failedLoginResponse = AppResponse.ErrorResult<AppUser>(string.Empty);
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(a => a.LoginAsync(It.Is<AppRequest<LoginModel>>(req => req.Data == loginModel)))
                .ReturnsAsync(failedLoginResponse)
                .Verifiable();
            var accountController = new AccountController(accountServiceMock.Object);

            var result = await accountController.Login(loginModel, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            accountServiceMock.Verify();
        }

        #endregion
    }
}
