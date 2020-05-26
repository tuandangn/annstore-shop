using Microsoft.AspNetCore.Mvc;

namespace Annstore.Web.Controllers
{
    public sealed class HomeController : PublishControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}