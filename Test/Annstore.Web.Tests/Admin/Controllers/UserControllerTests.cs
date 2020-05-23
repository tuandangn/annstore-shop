using Annstore.Web.Areas.Admin.Controllers;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.Tests.Admin.Controllers
{
    public class UserControllerTests
    {
        #region Index
        [Fact]
        public void Index_RedirectToList()
        {
            var userController = new UserController(Mock.Of<IAdminUserService>());

            var result = userController.Index();

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToList.ActionName);
        }

        #endregion

        #region List
        [Fact]
        public async Task List_ReturnViewWithValidModel()
        {
            var userListModel = new UserListModel(); 
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.GetUserListModelAsync(It.IsAny<UserListOptions>()))
                .ReturnsAsync(userListModel)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object);

            var result = await userController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(userListModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }
        #endregion
    }
}
