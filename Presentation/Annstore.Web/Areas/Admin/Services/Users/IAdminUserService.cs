using Annstore.Core.Entities.Users;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Services.Users
{
    public interface IAdminUserService
    {
        Task<UserModel> GetUserModelAsync(int id);

        Task<UserListModel> GetUserListModelAsync(UserListOptions opts);

        Task<AppResponse<AppUser>> CreateUserAsync(AppRequest<UserModel> request);

        Task<AppResponse<AppUser>> UpdateUserAsync(AppRequest<UserModel> request);

        Task<AppResponse> DeleteUserAsync(AppRequest<int> request);
    }
}
