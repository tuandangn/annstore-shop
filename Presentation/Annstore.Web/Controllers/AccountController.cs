using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Application.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Annstore.Web.Controllers
{
    public sealed class AccountController : PublishControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult Login(string returnUrl)
        {
            var model = new LoginModel();

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
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
    }
}
