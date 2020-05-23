using Microsoft.AspNetCore.Mvc;

namespace Annstore.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}