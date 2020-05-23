using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Areas.Admin.Services.Users.Options;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public sealed class UserController : Controller
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
    }
}
