using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Core.Entities.Customers;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Customers
{
    public interface IAdminCustomerService
    {
        Task<CustomerModel> GetCustomerModelAsync(int id);

        Task<CustomerListModel> GetCustomerListModelAsync(CustomerListOptions opts);

        Task<AppResponse<Customer>> CreateCustomerAsync(AppRequest<CustomerModel> request);

        Task<AppResponse<Customer>> UpdateCustomerAsync(AppRequest<CustomerModel> request);

        Task<AppResponse> DeleteCustomerAsync(AppRequest<int> request);
    }
}
