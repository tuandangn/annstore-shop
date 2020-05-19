using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Annstore.Services.Catalog;
using System.Linq;

namespace AnnstoreShop.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
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
            var categoryModels = categories.Select(category => category.ToSimpleModel()).ToList();

            var model = new CategoryListModel
            {
                Categories = categoryModels
            };

            return View(model);
        }

        public IActionResult Edit(int id) => View();

        public IActionResult Create() => View();
    }
}
