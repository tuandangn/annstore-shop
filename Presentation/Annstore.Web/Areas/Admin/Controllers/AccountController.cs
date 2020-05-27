using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Messages.Messages;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Application.Models.Admin.Accounts;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public sealed class AccountController : AdminControllerBase
    {
        private readonly IAdminAccountService _adminAccountService;
        private readonly IOptionsSnapshot<AccountSettings> _accountSettingsSnapshot;

        public AccountController(IAdminAccountService adminUserService, IOptionsSnapshot<AccountSettings> userSettingsSnapshot)
        {
            _adminAccountService = adminUserService;
            _accountSettingsSnapshot = userSettingsSnapshot;
        }

        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List(int page = 1, int size = 0)
        {
            if (page < 1) page = 1;
            if (size <= 0)
            {
                var accountSettings = _accountSettingsSnapshot.Value;
                size = accountSettings.Admin.DefaultPageSize;
            }
            var options = new AccountListOptions { PageNumber = page, PageSize = size };
            var model = await _adminAccountService.GetAccountListModelAsync(options);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var hasCustomers = await _adminAccountService.HasCustomersAsync();
            if (!hasCustomers)
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.Account.CustomerIsEmpty;
                return RedirectToAction(nameof(List));
            }

            var model = new AccountModel();
            await _adminAccountService.PrepareCustomersForAccountAsync(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<AccountModel>(model);
                var createResult = await _adminAccountService.CreateAccountAsync(request);

                if (createResult.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Account.CreateAccountSuccess;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, AdminMessages.Account.CreateAccountError);
            }
            await _adminAccountService.PrepareCustomersForAccountAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteAccountRequest = new AppRequest<int>(id);
            var deleteAccountResponse = await _adminAccountService.DeleteAccountAsync(deleteAccountRequest);

            if (deleteAccountResponse.Success)
            {
                TempData[AdminDefaults.SuccessMessage] = AdminMessages.Account.DeleteAccountSuccess;
            }
            else if (deleteAccountResponse.ModelIsInvalid)
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.Account.AccountIsNotFound;
            }
            else
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.Account.DeleteAccountError;
            }
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var hasCustomers = await _adminAccountService.HasCustomersAsync();
            if (!hasCustomers)
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.Account.CustomerIsEmpty;
                return RedirectToAction(nameof(List));
            }
            var model = await _adminAccountService.GetAccountModelAsync(id);

            if (model is NullAccountModel)
                return RedirectToAction(nameof(List));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountModel model)
        {
            if (ModelState.IsValid)
            {
                var updateAccountRequest = new AppRequest<AccountModel>(model);
                var updateAccountResponse = await _adminAccountService.UpdateAccountAsync(updateAccountRequest);

                if (updateAccountResponse.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Account.UpdateAccountSuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (updateAccountResponse.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = AdminMessages.Account.AccountIsNotFound;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, updateAccountResponse.Message);
            }
            await _adminAccountService.PrepareCustomersForAccountAsync(model);
            return View(model);
        }
    }
}
