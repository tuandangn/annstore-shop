using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Annstore.Services.Catalog;

namespace AnnstoreShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public sealed class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List()
        {
            var categories = await _categoryService.GetCategories();
            var model = new CategoryListModel
            {
                Categories = categories
            };

            return View(model);
        }
    }
}
