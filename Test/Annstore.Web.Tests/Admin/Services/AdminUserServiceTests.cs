using Annstore.Core.Entities.Users;
using Annstore.Services.Users;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using AutoMapper;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.Tests.Admin.Services
{
    public class AdminUserServiceTests
    {
        #region GetUserListModelAsync
        [Fact]
        public async Task GetUserListModelAsync_ReturnAllMappedUserSimpleModels()
        {
            var testUser = new AppUser { Id = 1 };
            var testModel = new UserSimpleModel { Id = testUser.Id };
            var users = new List<AppUser> { testUser };
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(u => u.GetUsersAsync())
                .ReturnsAsync(users)
                .Verifiable();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<UserSimpleModel>(testUser))
                .Returns(testModel)
                .Verifiable();
            var adminUserService = new AdminUserService(userServiceMock.Object, mapperMock.Object);
            var userListOptions = default(UserListOptions);

            var userListModel = await adminUserService.GetUserListModelAsync(userListOptions);

            Assert.Equal(users.Count, userListModel.Users.Count);
            Assert.Equal(testModel, userListModel.Users[0]);
            userServiceMock.Verify();
            mapperMock.Verify();
        }

        #endregion
    }
}
