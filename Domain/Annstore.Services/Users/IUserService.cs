using Annstore.Core.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Services.Users
{
    public interface IUserService
    {
        Task<List<AppUser>> GetUsersAsync();

        Task<IdentityResult> CreateUserAsync(AppUser user);
    }
}
