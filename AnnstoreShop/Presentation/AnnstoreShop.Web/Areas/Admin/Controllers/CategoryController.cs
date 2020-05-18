using Annstore.Core.Entities;
using Annstore.Data;
using Annstore.Services;
using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnstoreShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
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
