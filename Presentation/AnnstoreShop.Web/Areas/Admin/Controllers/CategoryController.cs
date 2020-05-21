using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Areas.Admin.Factories;
using Annstore.Web.Infrastructure;
using Microsoft.Extensions.Options;
using Annstore.Web.Areas.Admin.Services.Category.Options;

namespace Annstore.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public sealed class CategoryController : Controller
    {
        #region Fields
        private readonly IAdminCategoryService _adminCategoryService;
        private readonly IOptionsSnapshot<CategorySettings> _categorySettingsSnapshot;

        #endregion

        #region Ctor
        public CategoryController(IAdminCategoryService adminCategoryService, IOptionsSnapshot<CategorySettings> categorySettingsSnapshot)
        {
            _adminCategoryService = adminCategoryService;
            _categorySettingsSnapshot = categorySettingsSnapshot;
        }
        #endregion

        #region Actions
        public IActionResult Index() => RedirectToAction(nameof(List));

        public async Task<IActionResult> List()
        {
            //prepare list options
            var categorySettings = _categorySettingsSnapshot.Value;
            CategoryListOptions options = default(CategoryListOptions);
            options.PrepareBreadcrumb = true;
            options.BreadcrumbSeparator = categorySettings.AdminBreadcrumbSeparator;
            options.BreadcrumbDeepLevel = categorySettings.AdminBreadcrumbDeepLevel;
            options.BreadcrumbParentOnly = categorySettings.AdminBreadcrumbParentOnly;

            var model = await _adminCategoryService.GetCategoryListModelAsync(options);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _adminCategoryService.GetCategoryModelAsync(id);
            if (model == null)
                return RedirectToAction(nameof(List));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<CategoryModel>(model);
                var response = await _adminCategoryService.UpdateCategoryAsync(request);

                if (response.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Category.UpdateCategorySuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (response.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = response.Message;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CategoryModel();
            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new AppRequest<CategoryModel>(model);
                var response = await _adminCategoryService.CreateCategoryAsync(request);

                if (response.Success)
                {
                    TempData[AdminDefaults.SuccessMessage] = AdminMessages.Category.CreateCategorySuccess;
                    return RedirectToAction(nameof(List));
                }
                else if (response.ModelIsInvalid)
                {
                    TempData[AdminDefaults.ErrorMessage] = response.Message;
                    return RedirectToAction(nameof(List));
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }

            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = new AppRequest<int>(id);
            var response = await _adminCategoryService.DeleteCategoryAsync(request);

            if (response.Success)
            {
                TempData[AdminDefaults.SuccessMessage] = AdminMessages.Category.DeleteCategorySuccess;
            }
            else if (response.ModelIsInvalid)
            {
                TempData[AdminDefaults.ErrorMessage] = response.Message;
            }
            else
            {
                TempData[AdminDefaults.ErrorMessage] = response.Message;
            }
            return RedirectToAction(nameof(List));
        }
        #endregion
    }
}
