using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Annstore.Web.Areas.Admin.Controllers
{
    [Authorize]
    [Area(AreaNames.Admin)]
    public class AdminControllerBase : Controller
    {
    }
}
