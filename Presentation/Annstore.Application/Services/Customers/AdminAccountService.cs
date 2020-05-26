using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Annstore.Core.Common;
using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Common;
using Annstore.Auth.Entities;
using Annstore.Application.Infrastructure.Messages.Messages;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Services.Customers;
using Annstore.Application.Models.Admin.Customers;

namespace Annstore.Application.Services.Customers
{
    public sealed class AdminAccountService : IAdminAccountService
    {
        private readonly UserManager<Account> _userManager;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public AdminAccountService(UserManager<Account> userManager, ICustomerService customerService, IMapper mapper)
        {
            _userManager = userManager;
            _customerService = customerService;
            _mapper = mapper;
        }

        public async Task<AppResponse<Account>> CreateAccountAsync(AppRequest<AccountModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var account = _mapper.Map<Account>(request.Data);
            var insertResult = await _userManager.CreateAsync(account).ConfigureAwait(false);

            if (insertResult.Succeeded)
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(account, request.Data.Password).ConfigureAwait(false);

                if (addPasswordResult.Succeeded)
                    return AppResponse.SuccessResult(account);
            }
            return AppResponse.ErrorResult<Account>(AdminMessages.Account.CreateAccountError);
        }

        public async Task<AppResponse> DeleteAccountAsync(AppRequest<int> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var account = await _userManager.FindByIdAsync(request.Data.ToString()).ConfigureAwait(false);
            if (account == null)
                return AppResponse.InvalidModelResult(AdminMessages.Account.AccountIsNotFound);

            var deleteResult = await _userManager.DeleteAsync(account).ConfigureAwait(false);

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

            var accountQuery = from user in _userManager.Users
                            orderby user.Id
                            select user;
            //*TODO*
            var allAccounts = await accountQuery.ToListAsync().ConfigureAwait(false);
            var accounts = allAccounts.Skip((opts.PageNumber - 1) * opts.PageSize)
                .Take(opts.PageSize)
                .ToList();
            var pagedAccounts = accounts.ToPagedList(opts.PageSize, opts.PageNumber, allAccounts.Count);
            var accountModels = new List<AccountSimpleModel>();
            foreach (var account in pagedAccounts)
            {
                var accountModel = _mapper.Map<AccountSimpleModel>(account);
                var customer = await _customerService.GetCustomerByIdAsync(account.CustomerId);
                if (customer == null || customer.Deleted)
                    throw new Exception(AdminMessages.Customer.CustomerIsNotFound);
                accountModel.Customer = customer.FullName;
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
                return null;

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
            var account = await _userManager.FindByIdAsync(model.Id.ToString()).ConfigureAwait(false);
            if (account == null)
                return AppResponse.InvalidModelResult<Account>(AdminMessages.Account.AccountIsNotFound);
            account = _mapper.Map(model, account);
            var updateResult = await _userManager.UpdateAsync(account).ConfigureAwait(false);

            if (updateResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(account).ConfigureAwait(false);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(account, model.Password).ConfigureAwait(false);
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
