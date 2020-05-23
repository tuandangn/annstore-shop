using Annstore.Core.Entities.Users;
using Annstore.Web.Infrastructure;
using Annstore.Web.Models.Accounts;
using System.Threading.Tasks;

namespace Annstore.Web.Services.Accounts
{
    public interface IAccountService
    {
        Task<AppResponse<AppUser>> LoginAsync(AppRequest<LoginModel> model);
    }
}
