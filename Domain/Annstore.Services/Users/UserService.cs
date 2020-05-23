using Annstore.Core.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Services.Users
{
    public sealed class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateUserAsync(AppUser user)
        {
            if(user == null)
                throw new ArgumentNullException(nameof(user));

            var result = await _userManager.CreateAsync(user);

            return result;
        }

        public async Task<List<AppUser>> GetUsersAsync()
        {
            var query = from user in _userManager.Users
                        select user;

            return await query.ToListAsync();
        }
    }
}
