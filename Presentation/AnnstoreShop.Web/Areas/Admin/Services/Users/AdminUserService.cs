using Annstore.Core.Entities.Users;
using Annstore.Services.Users;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Services.Users
{
    public sealed class AdminUserService : IAdminUserService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AdminUserService(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<UserListModel> GetUserListModelAsync(UserListOptions opts)
        {
            var users = await _userService.GetUsersAsync();
            var userModels = new List<UserSimpleModel>();
            foreach(var user in users)
            {
                var userModel = _mapper.Map<UserSimpleModel>(user);
                userModels.Add(userModel);
            }
            var model = new UserListModel
            {
                Users = userModels
            };

            return model;
        }
    }
}
