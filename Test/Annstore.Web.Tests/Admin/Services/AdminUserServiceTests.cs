using Annstore.Core.Entities.Users;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Annstore.Web.Tests.Admin.Services
{
    public class AdminUserServiceTests
    {
        #region Helpers
        private Mock<UserManager<AppUser>> GetDefaultUserManager()
        {
            var userManagerStub = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
            return userManagerStub;
        }

        #endregion

        #region GetUserListModelAsync
        [Fact]
        public async Task GetUserListModelAsync_ReturnAllMappedUserSimpleModels()
        {
            var testUser = new AppUser { Id = 1 };
            var testModel = new UserSimpleModel { Id = testUser.Id };
            var users = new List<AppUser> { testUser };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(users.ToAsync())
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<UserSimpleModel>(testUser))
                .Returns(testModel)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperMock.Object);
            var userListOptions = default(UserListOptions);

            var userListModel = await adminUserService.GetUserListModelAsync(userListOptions);

            Assert.Equal(users.Count, userListModel.Users.Count);
            Assert.Equal(testModel, userListModel.Users[0]);
            userManagerMock.Verify();
            mapperMock.Verify();
        }

        #endregion

        #region CreateUserAsync
        [Fact]
        public async Task CreateUserAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminUserService = new AdminUserService(GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminUserService.CreateUserAsync(null));
        }

        [Fact]
        public async Task CreateUserAsync_CreateUserSuccessAndAddPasswordForUserSuccess_SuccessResponse()
        {
            var password = "password";
            var model = new UserModel { Id = 1, Password = password };
            var user = new AppUser { Id = 1 };
            var createSuccessResult = IdentityResult.Success;
            var addPasswordSuccessResult = IdentityResult.Success;
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.CreateAsync(user))
                .ReturnsAsync(createSuccessResult)
                .Verifiable();
            userManagerMock.Setup(u => u.AddPasswordAsync(user, password))
                .ReturnsAsync(addPasswordSuccessResult)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<AppUser>(model))
                .Returns(user)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperMock.Object);
            var request = new AppRequest<UserModel>(model);

            var successResult = await adminUserService.CreateUserAsync(request);

            Assert.True(successResult.Success);
            userManagerMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task CreateUserAsync_AddPasswordForUserFailed_ErrorResponse()
        {
            var password = "invalid password";
            var model = new UserModel { Id = 1, Password = password };
            var user = new AppUser { Id = 1 };
            var createSuccessResult = IdentityResult.Success;
            var addPasswordFailedResult = IdentityResult.Failed();
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.CreateAsync(user))
                .ReturnsAsync(createSuccessResult)
                .Verifiable();
            userManagerMock.Setup(u => u.AddPasswordAsync(user, password))
                .ReturnsAsync(addPasswordFailedResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<AppUser>(model))
                .Returns(user);
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperStub.Object);
            var request = new AppRequest<UserModel>(model);

            var successResult = await adminUserService.CreateUserAsync(request);

            Assert.False(successResult.Success);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task CreateUserAsync_CreateUserError_ReturnErrorResponse()
        {
            var model = new UserModel { Id = 1 };
            var user = new AppUser { Id = 1 };
            var createErrorResult = IdentityResult.Failed();
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.CreateAsync(user))
                .ReturnsAsync(createErrorResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<AppUser>(model))
                .Returns(user);
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperStub.Object);
            var request = new AppRequest<UserModel>(model);

            var errorResult = await adminUserService.CreateUserAsync(request);

            Assert.False(errorResult.Success);
            userManagerMock.Verify();
        }

        #endregion

        #region DeleteUserAsync
        [Fact]
        public async Task DeleteUserAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminUserService = new AdminUserService(GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminUserService.DeleteUserAsync(null));
        }

        [Fact]
        public async Task DeleteUserAsync_UserIsNotFound_ModelInvalidResponse()
        {
            var notFoundUserId = 0;
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByIdAsync(notFoundUserId.ToString()))
                .ReturnsAsync((AppUser)null)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, Mock.Of<IMapper>());
            var deleteUserRequest = new AppRequest<int>(notFoundUserId);

            var response = await adminUserService.DeleteUserAsync(deleteUserRequest);

            Assert.False(response.Success);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task DeleteUserAsync_DeleteUserError_ErrorResponse()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userManagerMock = GetDefaultUserManager();
            var deleteUserErrorResult = IdentityResult.Failed();
            userManagerMock.Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(u => u.DeleteAsync(user))
                .ReturnsAsync(deleteUserErrorResult)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, Mock.Of<IMapper>());
            var deleteUserRequest = new AppRequest<int>(userId);

            var response = await adminUserService.DeleteUserAsync(deleteUserRequest);

            Assert.False(response.Success);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task DeleteUserAsync_DeleteUserSuccess_SuccessResponse()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userManagerMock = GetDefaultUserManager();
            var deleteUserSuccessResult = IdentityResult.Success;
            userManagerMock.Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(u => u.DeleteAsync(user))
                .ReturnsAsync(deleteUserSuccessResult)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, Mock.Of<IMapper>());
            var deleteUserRequest = new AppRequest<int>(userId);

            var response = await adminUserService.DeleteUserAsync(deleteUserRequest);

            Assert.True(response.Success);
            userManagerMock.Verify();
        }

        #endregion
    }
}
