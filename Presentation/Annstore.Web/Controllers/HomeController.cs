using Annstore.Core.Events;
using Microsoft.AspNetCore.Mvc;

namespace Annstore.Web.Controllers
{
    public sealed class HomeController : PublishControllerBase
    {
        public HomeController(IEventPublisher publisher)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}