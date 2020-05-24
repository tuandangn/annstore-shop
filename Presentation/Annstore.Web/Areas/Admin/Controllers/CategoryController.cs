using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Infrastructure;
using Microsoft.Extensions.Options;
using Annstore.Web.Areas.Admin.Services.Categories;
using Annstore.Web.Areas.Admin.Services.Categories.Options;

namespace Annstore.Web.Areas.Admin.Controllers
{
    public sealed class CategoryController : AdminControllerBase
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

        public async Task<IActionResult> List(int page = 1, int size = 0)
        {
            var categorySettings = _categorySettingsSnapshot.Value;
            var categoryListOptions = _GetCategoryListOptions(categorySettings, page, size);
            var model = await _adminCategoryService.GetCategoryListModelAsync(categoryListOptions);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categorySettings = _categorySettingsSnapshot.Value;
            var breadcrumbOptions = _GetBreadcrumbOptions(categorySettings.Admin.Breadcrumb);
            var model = await _adminCategoryService.GetCategoryModelAsync(id, breadcrumbOptions);
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
            var categorySettings = _categorySettingsSnapshot.Value;
            var breadcrumbOptions = _GetBreadcrumbOptions(categorySettings.Admin.Breadcrumb);
            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions, true);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CategoryModel();
            //default values
            model.Published = true;
            model.DisplayOrder = 1;
            var categorySettings = _categorySettingsSnapshot.Value;
            var breadcrumbOptions = _GetBreadcrumbOptions(categorySettings.Admin.Breadcrumb);
            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions, true);

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

            var categorySettings = _categorySettingsSnapshot.Value;
            var breadcrumbOptions = _GetBreadcrumbOptions(categorySettings.Admin.Breadcrumb);
            await _adminCategoryService.PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOptions, true);
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

        #region Helpers
        private CategoryListOptions _GetCategoryListOptions(CategorySettings settings, int page, int size)
        {
            if (page < 1) page = 1;
            if (size <= 0) size = settings.Admin.DefaultPageSize;

            CategoryListOptions options = new CategoryListOptions();
            options.PageNumber = page;
            options.PageSize = size;
            options.Breadcrumb = _GetBreadcrumbOptions(settings.Admin.Breadcrumb);
            return options;
        }

        private BreadcrumbOptions _GetBreadcrumbOptions(CategorySettings.BreadcrumbSettings settings)
        {
            var breadcrumbOptions = new BreadcrumbOptions
            {
                Enable = settings.Enable,
                Separator = settings.Separator,
                DeepLevel = settings.DeepLevel,
                UseParentAsTarget = settings.UseParentAsTarget,
                ShowHidden = settings.ShowHidden
            };
            return breadcrumbOptions;
        }

        #endregion
    }
}
