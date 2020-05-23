using Annstore.Core.Entities.Users;
using Annstore.Services.Users;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Annstore.Services.Tests.Users
{
    public class UserServiceTests
    {
        #region Helpers
        private Mock<UserManager<AppUser>> GetDefaultUserManager()
        {
            var userManagerStub = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
            return userManagerStub;
        }

        #endregion

        #region CreateUserAsync
        [Fact]
        public async Task CreateUserAsync_UserIsNull_ThrowArgumentNullException()
        {
            var userService = new UserService(GetDefaultUserManager().Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => userService.CreateUserAsync(null));
        }
        
        [Fact]
        public async Task CreateUserAsync_InsertUserSuccess_ReturnSuccessResult()
        {
            var user = new AppUser();
            var userManagerMock = GetDefaultUserManager();
            var successResult = IdentityResult.Success;
            userManagerMock.Setup(u => u.CreateAsync(user))
                .ReturnsAsync(successResult)
                .Verifiable();
            var userService = new UserService(userManagerMock.Object);

            var result = await userService.CreateUserAsync(user);

            Assert.Equal(successResult, result);
            userManagerMock.Verify();
        }
        
        [Fact]
        public async Task CreateUserAsync_InsertUserFailed_ReturnFailedResult()
        {
            var user = new AppUser();
            var userManagerMock = GetDefaultUserManager();
            var failedResult = IdentityResult.Failed();
            userManagerMock.Setup(u => u.CreateAsync(user))
                .ReturnsAsync(failedResult)
                .Verifiable();
            var userService = new UserService(userManagerMock.Object);

            var result = await userService.CreateUserAsync(user);

            Assert.Equal(failedResult, result);
            userManagerMock.Verify();
        }

        #endregion

        #region GetUsersAsync
        [Fact]
        public async Task GetUsersAsync_ReturnAllUsers()
        {
            var availableUsers = new List<AppUser> { new AppUser() };
            var userManagerMock = GetDefaultUserManager();
            userManagerMock.Setup(u => u.Users)
                .Returns(availableUsers.ToAsync())
                .Verifiable();
            var userService = new UserService(userManagerMock.Object);

            var result = await userService.GetUsersAsync();

            Assert.Equal(availableUsers.Count, result.Count);
            userManagerMock.Verify();
        }

        #endregion
    }
}
