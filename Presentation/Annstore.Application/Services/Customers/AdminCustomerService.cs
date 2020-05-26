using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Messages.Messages;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Application.Services.Customers;
using Annstore.Core.Entities.Customers;
using Annstore.Services.Customers;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Categories
{
    public sealed class AdminCustomerService : IAdminCustomerService
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public AdminCustomerService(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        public async Task<CustomerListModel> GetCustomerListModelAsync(CustomerListOptions options)
        {
            var pagedCustomers = await _customerService.GetPagedCustomersAsync(
                options.PageNumber,
                options.PageSize
            ).ConfigureAwait(false);
            var customerModels = new List<CustomerSimpleModel>();
            foreach (var customer in pagedCustomers)
            {
                var simpleModel = _mapper.Map<CustomerSimpleModel>(customer);
                customerModels.Add(simpleModel);
            }

            var model = new CustomerListModel
            {
                Customers = customerModels,
                PageNumber = pagedCustomers.PageNumber,
                PageSize = pagedCustomers.PageSize,
                TotalItems = pagedCustomers.TotalItems,
                TotalPages = pagedCustomers.TotalPages
            };

            return model;
        }

        public async Task<CustomerModel> GetCustomerModelAsync(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id)
                .ConfigureAwait(false);
            if (customer == null || customer.Deleted)
                return null;

            var model = _mapper.Map<CustomerModel>(customer);

            return model;
        }

        public async Task<AppResponse<Customer>> CreateCustomerAsync(AppRequest<CustomerModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var customer = _mapper.Map<Customer>(request.Data);
                await _customerService.CreateCustomerAsync(customer).ConfigureAwait(false);
                return AppResponse.SuccessResult(customer);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult<Customer>(AdminMessages.Customer.CreateCustomerError);
            }
        }

        public async Task<AppResponse<Customer>> UpdateCustomerAsync(AppRequest<CustomerModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var customer = await _customerService.GetCustomerByIdAsync(request.Data.Id)
                .ConfigureAwait(false);
            if (customer == null || customer.Deleted)
                return AppResponse.InvalidModelResult<Customer>(AdminMessages.Customer.CustomerIsNotFound);

            try
            {
                customer = _mapper.Map(request.Data, customer);
                await _customerService.UpdateCustomerAsync(customer).ConfigureAwait(false);

                return AppResponse.SuccessResult(customer);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult<Customer>(AdminMessages.Customer.UpdateCustomerError);
            }
        }

        public async Task<AppResponse> DeleteCustomerAsync(AppRequest<int> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var customer = await _customerService.GetCustomerByIdAsync(request.Data).ConfigureAwait(false);
            if (customer == null)
                return AppResponse.InvalidModelResult(AdminMessages.Customer.CustomerIsNotFound);

            try
            {
                if (!customer.Deleted)
                {
                    await _customerService.DeleteCustomerAsync(customer).ConfigureAwait(false);
                }

                return AppResponse.SuccessResult(AdminMessages.Customer.DeleteCustomerSuccess);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult(AdminMessages.Customer.DeleteCustomerError);
            }
        }
    }
}
