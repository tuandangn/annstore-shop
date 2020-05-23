using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public sealed class UserController : AdminControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public UserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List()
        {
            var options = new UserListOptions();
            var model = await _adminUserService.GetUserListModelAsync(options);

            return View(model);
        }

        public IActionResult Create()
        {
            var model = new UserModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<UserModel>(model);
                var createResult = await _adminUserService.CreateUserAsync(request);

                if (createResult.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.User.CreateUserSuccess;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, AdminMessages.User.CreateUserError);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteUserRequest = new AppRequest<int>(id);
            var deleteUserResponse = await _adminUserService.DeleteUserAsync(deleteUserRequest);

            if (deleteUserResponse.Success)
            {
                TempData[AdminDefaults.SuccessMessage] = AdminMessages.User.DeleteUserSuccess;
            }
            else if (deleteUserResponse.ModelIsInvalid)
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.User.UserIsNotFound;
            }
            else
            {
                TempData[AdminDefaults.ErrorMessage] = AdminMessages.User.DeleteUserError;
            }
            return RedirectToAction(nameof(List));
        }
    }
}
