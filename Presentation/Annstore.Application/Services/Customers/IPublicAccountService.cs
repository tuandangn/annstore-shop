using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Auth.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Customers
{
    public interface IPublicAccountService
    {
        Task<AppResponse<Account>> LoginAsync(AppRequest<LoginModel> model);

        Task LogoutAsync(ClaimsPrincipal principal);
    }
}
