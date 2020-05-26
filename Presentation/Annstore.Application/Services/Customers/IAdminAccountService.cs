using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Application.Models.Admin.Common;
using Annstore.Auth.Entities;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Customers
{
    public interface IAdminAccountService
    {
        Task<bool> HasCustomersAsync();

        Task<AccountModel> GetAccountModelAsync(int id);

        Task<AccountModel> PrepareCustomersForAccountAsync(AccountModel model);

        Task<AccountListModel> GetAccountListModelAsync(AccountListOptions opts);

        Task<AppResponse<Account>> CreateAccountAsync(AppRequest<AccountModel> request);

        Task<AppResponse<Account>> UpdateAccountAsync(AppRequest<AccountModel> request);

        Task<AppResponse> DeleteAccountAsync(AppRequest<int> request);
    }
}
