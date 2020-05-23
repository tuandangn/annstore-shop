using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Services.Users
{
    public interface IAdminUserService
    {
        Task<UserListModel> GetUserListModelAsync(UserListOptions opts);
    }
}
