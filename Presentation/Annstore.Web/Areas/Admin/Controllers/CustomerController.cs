using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Common;
using Annstore.Application.Services.Customers;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Application.Infrastructure.Messages.Messages;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public sealed class CustomerController : AdminControllerBase
    {
        #region Fields
        private readonly IAdminCustomerService _adminCustomerService;
        private readonly IOptionsSnapshot<CustomerSettings> _customerSettingsSnapshot;

        #endregion

        #region Ctor
        public CustomerController(IAdminCustomerService adminCustomerService, IOptionsSnapshot<CustomerSettings> customerSettingsSnapshot)
        {
            _adminCustomerService = adminCustomerService;
            _customerSettingsSnapshot = customerSettingsSnapshot;
        }
        #endregion

        #region Actions
        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List(int page = 1, int size = 0)
        {
            var customerSettings = _customerSettingsSnapshot.Value;
            var customerListOptions = _GetCustomerListOptions(customerSettings, page, size);
            var model = await _adminCustomerService.GetCustomerListModelAsync(customerListOptions);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customerSettings = _customerSettingsSnapshot.Value;
            var model = await _adminCustomerService.GetCustomerModelAsync(id);
            if (model is NullCustomerModel)
                return RedirectToAction(nameof(List));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<CustomerModel>(model);
                var response = await _adminCustomerService.UpdateCustomerAsync(request);

                if (response.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Customer.UpdateCustomerSuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (response.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = response.Message;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            return View(model);
        }

        public IActionResult Create()
        {
            var model = new CustomerModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<CustomerModel>(model);
                var response = await _adminCustomerService.CreateCustomerAsync(request);

                if (response.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Customer.CreateCustomerSuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (response.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = response.Message;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = new AppRequest<int>(id);
            var response = await _adminCustomerService.DeleteCustomerAsync(request);

            if (response.Success)
            {
                TempData[AdminDefaults.SuccessMessage] = AdminMessages.Customer.DeleteCustomerSuccess;
            }
            else if (response.ModelIsInvalid)
            {
                TempData[AdminDefaults.ErrorMessage] = response.Message;
            }
            else
            {
                TempData[AdminDefaults.ErrorMessage] = response.Message;
            }
            return RedirectToAction(nameof(List));
        }
        #endregion

        #region Helpers
        private CustomerListOptions _GetCustomerListOptions(CustomerSettings settings, int page, int size)
        {
            if (page < 1) page = 1;
            if (size <= 0) size = settings.Admin.DefaultPageSize;

            CustomerListOptions options = new CustomerListOptions();
            options.PageNumber = page;
            options.PageSize = size;
            return options;
        }
        #endregion
    }
}
