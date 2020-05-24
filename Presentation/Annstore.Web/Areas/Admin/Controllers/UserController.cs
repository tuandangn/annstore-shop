using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Areas.Admin.Models.Users;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Annstore.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public sealed class UserController : AdminControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IOptionsSnapshot<UserSettings> _userSettingsSnapshot;

        public UserController(IAdminUserService adminUserService, IOptionsSnapshot<UserSettings> userSettingsSnapshot)
        {
            _adminUserService = adminUserService;
            _userSettingsSnapshot = userSettingsSnapshot;
        }

        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List(int page = 1, int size = 0)
        {
            if (page < 1) page = 1;
            if (size <= 0)
            {
                var userSettings = _userSettingsSnapshot.Value;
                size = userSettings.Admin.DefaultPageSize;
            }
            var options = new UserListOptions { PageNumber = page, PageSize = size };
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

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _adminUserService.GetUserModelAsync(id);

            if (model == null)
                return RedirectToAction(nameof(List));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var updateUserRequest = new AppRequest<UserModel>(model);
                var updateUserResponse = await _adminUserService.UpdateUserAsync(updateUserRequest);

                if (updateUserResponse.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.User.UpdateUserSuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (updateUserResponse.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = AdminMessages.User.UserIsNotFound;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, updateUserResponse.Message);
            }
            return View(model);
        }
    }
}
