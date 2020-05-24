using Annstore.Core.Entities.Catalog;
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
using System.Linq;
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
        public async Task GetUserListModelAsync_OptionPageNumberLessThanOne_ThrowArgumentException()
        {
            var pageNumber = 0;
            var userListOptions = new UserListOptions { PageNumber = pageNumber };
            var userManagerStub = GetDefaultUserManager();
            var adminUserService = new AdminUserService(userManagerStub.Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentException>(() => adminUserService.GetUserListModelAsync(userListOptions));
        }

        [Fact]
        public async Task GetUserListModelAsync_OptionPageSizeLessThanOrEqualZero_ThrowArgumentException()
        {
            var pageSize = 0;
            var userListOptions = new UserListOptions { PageNumber = 1, PageSize = pageSize };
            var userManagerStub = GetDefaultUserManager();
            var adminUserService = new AdminUserService(userManagerStub.Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentException>(() => adminUserService.GetUserListModelAsync(userListOptions));
        }

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
            var userListOptions = new UserListOptions { PageNumber = 1, PageSize = int.MaxValue };

            var userListModel = await adminUserService.GetUserListModelAsync(userListOptions);

            Assert.Equal(users.Count, userListModel.Users.Count);
            Assert.Equal(testModel, userListModel.Users[0]);
            userManagerMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task GetUserListModelAsync_Pagination_PagedUserSimpleModels()
        {
            var pageNumber = 2;
            var pageSize = 3;
            var testUser = new AppUser { Id = 4 };
            var testModel = new UserSimpleModel { Id = testUser.Id };
            var users = new List<AppUser> {
                new AppUser{Id = 1},new AppUser{Id = 2},new AppUser{Id = 3},
                testUser
            };
            var userListOptions = new UserListOptions { PageNumber = pageNumber, PageSize = pageSize };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(users.ToAsync())
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<UserSimpleModel>(testUser))
                .Returns(testModel);
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperStub.Object);

            var userListModel = await adminUserService.GetUserListModelAsync(userListOptions);

            Assert.Equal(2, userListModel.TotalPages);
            Assert.Equal(1, userListModel.Users.Count);
            Assert.Equal(testUser.Id, userListModel.Users.ElementAt(0).Id);
            userManagerMock.Verify();
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

        #region GetUserModelAsync
        [Fact]
        public async Task GetUserModelAsync_UserIsNotFound_ReturnNull()
        {
            var notFoundUserId = 0;
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByIdAsync(notFoundUserId.ToString()))
                .ReturnsAsync((AppUser)null)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, Mock.Of<IMapper>());

            var result = await adminUserService.GetUserModelAsync(notFoundUserId);

            Assert.Null(result);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task GetUserModelAsync_UserIsFound_ReturnMappedModel()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userModel = new UserModel { Id = userId };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<UserModel>(user))
                .Returns(userModel)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperMock.Object);

            var result = await adminUserService.GetUserModelAsync(userId);

            Assert.Equal(userModel, result);
            mapperMock.Verify();
        }

        #endregion

        #region UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_RequestIsNull_ThrowArgumentNullException()
        {
            var adminUserService = new AdminUserService(GetDefaultUserManager().Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ArgumentNullException>(() => adminUserService.UpdateUserAsync(null));
        }

        [Fact]
        public async Task UpdateUserAsync_UserIsNotFound_ModelInvalidResponse()
        {
            var notFoundUserId = 0;
            var userModel = new UserModel { Id = notFoundUserId };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.FindByIdAsync(notFoundUserId.ToString()))
                .ReturnsAsync((AppUser)null)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, Mock.Of<IMapper>());
            var updateUserRequest = new AppRequest<UserModel>(userModel);

            var response = await adminUserService.UpdateUserAsync(updateUserRequest);

            Assert.False(response.Success);
            Assert.True(response.ModelIsInvalid);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task UpdateUserAsync_UpdateUserSuccess_SuccessResponse()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userModel = new UserModel { Id = userId };
            var mappedUser = new AppUser { Id = userId };
            var updateUserResult = IdentityResult.Success;
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(m => m.UpdateAsync(mappedUser))
                .ReturnsAsync(updateUserResult)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<UserModel, AppUser>(userModel, user))
                .Returns(mappedUser)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperMock.Object);
            var updateUserRequest = new AppRequest<UserModel>(userModel);

            var response = await adminUserService.UpdateUserAsync(updateUserRequest);

            Assert.True(response.Success);
            userManagerMock.Verify();
            mapperMock.Verify();
        }

        [Fact]
        public async Task UpdateUserAsync_RemovePasswordError_ErrorResponse()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userModel = new UserModel { Id = userId, Password = "change password" };
            var mappedUser = new AppUser { Id = userId };
            var updateUserResult = IdentityResult.Success;
            var removePasswordResult = IdentityResult.Failed();
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(m => m.UpdateAsync(mappedUser))
                .ReturnsAsync(updateUserResult);
            userManagerMock.Setup(m => m.RemovePasswordAsync(mappedUser))
                .ReturnsAsync(removePasswordResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<UserModel, AppUser>(userModel, user))
                .Returns(mappedUser)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperStub.Object);
            var updateUserRequest = new AppRequest<UserModel>(userModel);

            var response = await adminUserService.UpdateUserAsync(updateUserRequest);

            Assert.False(response.Success);
            userManagerMock.Verify();
        }

        [Fact]
        public async Task UpdateUserAsync_AddPasswordError_ErrorResponse()
        {
            var userId = 1;
            var user = new AppUser { Id = userId };
            var userModel = new UserModel { Id = userId, Password = "change password" };
            var mappedUser = new AppUser { Id = userId };
            var updateUserResult = IdentityResult.Success;
            var removePasswordResult = IdentityResult.Success;
            var addPasswordResult = IdentityResult.Failed();
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(m => m.UpdateAsync(mappedUser))
                .ReturnsAsync(updateUserResult);
            userManagerMock.Setup(m => m.RemovePasswordAsync(mappedUser))
                .ReturnsAsync(removePasswordResult);
            userManagerMock.Setup(m => m.AddPasswordAsync(mappedUser, userModel.Password))
                .ReturnsAsync(addPasswordResult)
                .Verifiable();
            var mapperStub = new Mock<IMapper>();
            mapperStub.Setup(m => m.Map<UserModel, AppUser>(userModel, user))
                .Returns(mappedUser)
                .Verifiable();
            var adminUserService = new AdminUserService(userManagerMock.Object, mapperStub.Object);
            var updateUserRequest = new AppRequest<UserModel>(userModel);

            var response = await adminUserService.UpdateUserAsync(updateUserRequest);

            Assert.False(response.Success);
            userManagerMock.Verify();
        }

        #endregion
    }
}
