﻿using Annstore.Core.Entities.Users;
using Annstore.Web.Areas.Admin.Controllers;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.Tests.Admin.Controllers
{
    public class UserControllerTests
    {
        #region Helper
        private IOptionsSnapshot<UserSettings> GetDefaultUserOptions(int defaultPageSize = int.MaxValue)
        {
            var userSettings = new UserSettings
            {
                Admin = new UserSettings.AdminUserSettings { DefaultPageSize = defaultPageSize }
            };
            var userSettingsSnapshotStub = new Mock<IOptionsSnapshot<UserSettings>>();
            userSettingsSnapshotStub.Setup(u => u.Value)
                .Returns(userSettings);

            return userSettingsSnapshotStub.Object;
        }

        #endregion

        #region Index
        [Fact]
        public void Index_RedirectToList()
        {
            var userController = new UserController(Mock.Of<IAdminUserService>(), GetDefaultUserOptions());

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
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());

            var result = await userController.List();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(userListModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task List_PaginationInfoInvalid_UseDefaultValues()
        {
            var defaultPageSize = 12;
            var page = 0;
            var size = 0;
            var userListModel = new UserListModel();
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.GetUserListModelAsync(
                It.Is<UserListOptions>(opts =>
                    opts.PageNumber == 1 && opts.PageSize == defaultPageSize
                )))
                .ReturnsAsync(userListModel)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions(defaultPageSize));

            var result = await userController.List(page, size);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(userListModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }

        #endregion

        #region Create
        [Fact]
        public void Create_ReturnValidModel()
        {
            var userController = new UserController(Mock.Of<IAdminUserService>(), GetDefaultUserOptions());

            var result = userController.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.Model);
        }

        #endregion

        #region Create Post
        [Fact]
        public async Task CreatePost_ModelIsInvalid_RedisplayView()
        {
            var invalidUserModel = new UserModel();
            var userController = new UserController(Mock.Of<IAdminUserService>(), GetDefaultUserOptions());
            userController.ModelState.AddModelError("error", "error");

            var result = await userController.Create(invalidUserModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidUserModel, viewResult.Model);
        }

        [Fact]
        public async Task CreatePost_CreateUserSuccess_RedirectToList()
        {
            var userModel = new UserModel();
            var user = new AppUser();
            var createSuccessResponse = AppResponse.SuccessResult<AppUser>(user);
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(u => u.CreateUserAsync(It.Is<AppRequest<UserModel>>(req => req.Data == userModel)))
                .ReturnsAsync(createSuccessResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Create(userModel);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToList.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task CreatePost_CreateUserError_RedisplayView()
        {
            var userModel = new UserModel();
            var user = new AppUser();
            var createErrorResponse = AppResponse.ErrorResult<AppUser>(string.Empty);
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(u => u.CreateUserAsync(It.Is<AppRequest<UserModel>>(req => req.Data == userModel)))
                .ReturnsAsync(createErrorResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Create(userModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(userModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }

        #endregion

        #region Delete Post
        [Fact]
        public async Task DeletePost_UserIsNotFound_RedirectToList()
        {
            var notFoundUserId = 0;
            var deleteUserModelInvalidResponse = AppResponse.InvalidModelResult("model invalid");
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.DeleteUserAsync(It.Is<AppRequest<int>>(req => req.Data == notFoundUserId)))
                .ReturnsAsync(deleteUserModelInvalidResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Delete(notFoundUserId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToList.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task DeletePost_DeleteUserError_RedirectToList()
        {
            var deleteUserId = 1;
            var deleteUserErrorResponse = AppResponse.ErrorResult("delete user error");
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.DeleteUserAsync(It.Is<AppRequest<int>>(req => req.Data == deleteUserId)))
                .ReturnsAsync(deleteUserErrorResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Delete(deleteUserId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToList.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task DeletePost_DeleteUserSuccc_RedirectToList()
        {
            var deleteUserId = 1;
            var deleteUserSuccessResponse = AppResponse.ErrorResult("delete user error");
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.DeleteUserAsync(It.Is<AppRequest<int>>(req => req.Data == deleteUserId)))
                .ReturnsAsync(deleteUserSuccessResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Delete(deleteUserId);

            var redirectToList = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToList.ActionName);
            adminUserServiceMock.Verify();
        }

        #endregion

        #region Edit
        [Fact]
        public async Task Edit_UserModelIsNull_RedirectToList()
        {
            var userId = 0;
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.GetUserModelAsync(userId))
                .ReturnsAsync((UserModel)null)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());

            var result = await userController.Edit(userId);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToAction.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_UserModelIsNotNull_ReturnView()
        {
            var userId = 1;
            var userModel = new UserModel { Id = userId };
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.GetUserModelAsync(userId))
                .ReturnsAsync(userModel)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());

            var result = await userController.Edit(userId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(userModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }

        #endregion

        #region Edit Post
        [Fact]
        public async Task EditPost_ModelStateIsInvalid_RedisplayView()
        {
            var userModel = new UserModel();
            var userController = new UserController(Mock.Of<IAdminUserService>(), GetDefaultUserOptions());
            userController.ModelState.AddModelError("error", "error");

            var result = await userController.Edit(userModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(userModel, viewResult.Model);
        }

        [Fact]
        public async Task EditPost_ModelInvalidResponse_RedirectToList()
        {
            var userModel = new UserModel();
            var modelInvalidResponse = AppResponse.InvalidModelResult<AppUser>("model invalid");
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.UpdateUserAsync(It.Is<AppRequest<UserModel>>(req => req.Data == userModel)))
                .ReturnsAsync(modelInvalidResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Edit(userModel);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToAction.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_UpdateUserIsSuccess_RedirectToList()
        {
            var userModel = new UserModel();
            var updateUserSuccessResponse = AppResponse.SuccessResult(new AppUser());
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.UpdateUserAsync(It.Is<AppRequest<UserModel>>(req => req.Data == userModel)))
                .ReturnsAsync(updateUserSuccessResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Edit(userModel);

            var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.List), redirectToAction.ActionName);
            adminUserServiceMock.Verify();
        }

        [Fact]
        public async Task EditPost_UpdateUserError_RedisplayView()
        {
            var userModel = new UserModel();
            var updateUserErrorResponse = AppResponse.ErrorResult<AppUser>("update error");
            var adminUserServiceMock = new Mock<IAdminUserService>();
            adminUserServiceMock.Setup(a => a.UpdateUserAsync(It.Is<AppRequest<UserModel>>(req => req.Data == userModel)))
                .ReturnsAsync(updateUserErrorResponse)
                .Verifiable();
            var userController = new UserController(adminUserServiceMock.Object, GetDefaultUserOptions());
            userController.TempData = Mock.Of<ITempDataDictionary>();

            var result = await userController.Edit(userModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.Equal(userModel, viewResult.Model);
            adminUserServiceMock.Verify();
        }

        #endregion
    }
}
