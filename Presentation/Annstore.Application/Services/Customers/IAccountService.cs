using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Auth.Entities;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Customers
{
    public interface IAccountService
    {
        Task<AppResponse<Account>> LoginAsync(AppRequest<LoginModel> model);
    }
}
