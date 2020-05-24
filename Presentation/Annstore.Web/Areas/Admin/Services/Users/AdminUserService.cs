using Annstore.Core.Entities.Users;
using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Annstore.Core.Common;

namespace Annstore.Web.Areas.Admin.Services.Users
{
    public sealed class AdminUserService : IAdminUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AdminUserService(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<AppResponse<AppUser>> CreateUserAsync(AppRequest<UserModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = _mapper.Map<AppUser>(request.Data);
            var insertResult = await _userManager.CreateAsync(user).ConfigureAwait(false);

            if (insertResult.Succeeded)
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Data.Password);

                if (addPasswordResult.Succeeded)
                    return AppResponse.SuccessResult(user);
            }
            return AppResponse.ErrorResult<AppUser>(AdminMessages.User.CreateUserError);
        }

        public async Task<AppResponse> DeleteUserAsync(AppRequest<int> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userManager.FindByIdAsync(request.Data.ToString());
            if (user == null)
                return AppResponse.InvalidModelResult(AdminMessages.User.UserIsNotFound);

            var deleteResult = await _userManager.DeleteAsync(user);

            if (deleteResult.Succeeded)
                return AppResponse.SuccessResult();
            return AppResponse.ErrorResult(AdminMessages.User.DeleteUserError);
        }

        public async Task<UserListModel> GetUserListModelAsync(UserListOptions opts)
        {
            if (opts.PageNumber < 1)
                throw new ArgumentException("Page number must greater than or equal 1");
            if (opts.PageSize <= 0)
                throw new ArgumentException("Page size must greater than 0");

            var userQuery = from user in _userManager.Users
                            orderby user.Id
                            select user;
            //*TODO*
            var allUsers = await userQuery.ToListAsync().ConfigureAwait(false);
            var users = allUsers.Skip((opts.PageNumber - 1) * opts.PageSize)
                .Take(opts.PageSize)
                .ToList();
            var pagedUsers = users.ToPagedList(opts.PageSize, opts.PageNumber, allUsers.Count);
            var userModels = new List<UserSimpleModel>();
            foreach (var user in pagedUsers)
            {
                var userModel = _mapper.Map<UserSimpleModel>(user);
                userModels.Add(userModel);
            }
            var model = new UserListModel
            {
                Users = userModels,
                PageNumber = pagedUsers.PageNumber,
                PageSize = pagedUsers.PageSize,
                TotalItems = pagedUsers.TotalItems,
                TotalPages = pagedUsers.TotalPages
            };

            return model;
        }
    }
}
