using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Application.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Annstore.Web.Controllers
{
    public sealed class AccountController : PublishControllerBase
    {
        private readonly IPublicAccountService _accountService;

        public AccountController(IPublicAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult SignIn(string returnUrl)
        {
            var model = new LoginModel();

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var loginRequest = new AppRequest<LoginModel>(model);
                var loginResponse = await _accountService.LoginAsync(loginRequest);

                if (loginResponse.Success)
                {
                    if(string.IsNullOrEmpty(returnUrl))
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, loginResponse.Message);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await _accountService.LogoutAsync(User);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
