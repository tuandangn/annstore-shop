using Microsoft.AspNetCore.Mvc;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public class HomeController : AdminControllerBase
    {
        public IActionResult Index => View();
    }
}
