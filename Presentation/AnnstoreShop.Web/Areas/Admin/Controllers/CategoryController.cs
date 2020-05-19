using System;
using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Annstore.Services.Catalog;
using System.Linq;
using AutoMapper;
using Annstore.Core.Entities.Catalog;
using AnnstoreShop.Web.Areas.Admin.Infrastructure;

namespace AnnstoreShop.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public sealed class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        
        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryModels = categories.Select(category => _mapper.Map<CategorySimpleModel>(category)).ToList();

            var model = new CategoryListModel
            {
                Categories = categoryModels
            };

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if(category == null)
                return RedirectToAction(nameof(List));

            var model = _mapper.Map<CategoryModel>(category);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            if (category == null)
                return RedirectToAction(nameof(List));

            category = _mapper.Map<CategoryModel, Category>(model, category);            
            await _categoryService.UpdateCategory(category);

            TempData[AdminDefaults.SuccessMessage] = $"Chỉnh sửa danh mục {category.Name} thành công!";

            return RedirectToAction(nameof(List));
        }

        public IActionResult Create() => View();
    }
}
