using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Infrastructure.Messages.Messages;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Services.Customers;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Auth.Entities;
using Annstore.Auth.Services;

namespace Annstore.Application.Services.Customers
{
    public sealed class AdminAccountService : IAdminAccountService
    {
        private readonly UserManager<Account> _userManager;
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AdminAccountService(UserManager<Account> userManager, IAccountService accountService,
            ICustomerService customerService, IMapper mapper)
        {
            _userManager = userManager;
            _customerService = customerService;
            _mapper = mapper;
            _accountService = accountService;
        }

        public async Task<AppResponse<Account>> CreateAccountAsync(AppRequest<AccountModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var account = _mapper.Map<Account>(request.Data);
            var insertResult = await _userManager.CreateAsync(account)
                .ConfigureAwait(false);

            if (insertResult.Succeeded)
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(account, request.Data.Password)
                    .ConfigureAwait(false);

                if (addPasswordResult.Succeeded)
                    return AppResponse.SuccessResult(account);
            }
            return AppResponse.ErrorResult<Account>(AdminMessages.Account.CreateAccountError);
        }

        public async Task<AppResponse> DeleteAccountAsync(AppRequest<int> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var account = await _userManager.FindByIdAsync(request.Data.ToString())
                .ConfigureAwait(false);
            if (account == null)
                return AppResponse.InvalidModelResult(AdminMessages.Account.AccountIsNotFound);

            var deleteResult = await _userManager.DeleteAsync(account)
                .ConfigureAwait(false);

            if (deleteResult.Succeeded)
                return AppResponse.SuccessResult();
            return AppResponse.ErrorResult(AdminMessages.Account.DeleteAccountError);
        }

        public async Task<AccountListModel> GetAccountListModelAsync(AccountListOptions opts)
        {
            if (opts.PageNumber < 1)
                throw new ArgumentException("Page number must greater than or equal 1");
            if (opts.PageSize <= 0)
                throw new ArgumentException("Page size must greater than 0");

            var pagedAccounts = await _accountService.GetPagedAccountsAsync(opts.PageNumber, opts.PageSize)
                .ConfigureAwait(false);
            var accountModels = new List<AccountSimpleModel>();
            foreach (var account in pagedAccounts)
            {
                var customer = await _customerService.GetCustomerByIdAsync(account.CustomerId)
                    .ConfigureAwait(false);
                if (customer == null || customer.Deleted)
                    throw new Exception(AdminMessages.Customer.CustomerIsNotFound);
                var accountModel = _mapper.Map<AccountSimpleModel>(account);
                accountModel.Customer = _mapper.Map<CustomerSimpleModel>(customer);
                accountModels.Add(accountModel);
            }
            var model = new AccountListModel
            {
                Accounts = accountModels,
                PageNumber = pagedAccounts.PageNumber,
                PageSize = pagedAccounts.PageSize,
                TotalItems = pagedAccounts.TotalItems,
                TotalPages = pagedAccounts.TotalPages
            };

            return model;
        }

        public async Task<AccountModel> GetAccountModelAsync(int id)
        {
            var account = await _userManager.FindByIdAsync(id.ToString())
                .ConfigureAwait(false);
            if (account == null)
                return AccountModel.NullModel;

            var model = _mapper.Map<AccountModel>(account);
            await PrepareCustomersForAccountAsync(model)
                .ConfigureAwait(false);

            return model;
        }

        public async Task<bool> HasCustomersAsync()
        {
            return await _customerService.HasCustomersAsync()
                .ConfigureAwait(false);
        }

        public async Task<AccountModel> PrepareCustomersForAccountAsync(AccountModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var customers = await _customerService.GetCustomersAsync()
                .ConfigureAwait(false);
            model.Customers = customers.Select(customer => _mapper.Map<CustomerSimpleModel>(customer))
                .ToList();

            return model;
        }

        public async Task<AppResponse<Account>> UpdateAccountAsync(AppRequest<AccountModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var model = request.Data;
            var account = await _userManager.FindByIdAsync(model.Id.ToString())
                .ConfigureAwait(false);
            if (account == null)
                return AppResponse.InvalidModelResult<Account>(AdminMessages.Account.AccountIsNotFound);
            account = _mapper.Map(model, account);
            var updateResult = await _userManager.UpdateAsync(account)
                .ConfigureAwait(false);

            if (updateResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(account)
                        .ConfigureAwait(false);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(account, model.Password)
                            .ConfigureAwait(false);
                        if (addPasswordResult.Succeeded)
                        {
                            return AppResponse.SuccessResult(account);
                        }
                    }
                    return AppResponse.ErrorResult<Account>(AdminMessages.Account.UpdateAccountError);
                }
                return AppResponse.SuccessResult(account);
            }
            return AppResponse.ErrorResult<Account>(AdminMessages.Account.UpdateAccountError);
        }
    }
}
