using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using Annstore.Web.Areas.Admin.Infrastructure;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Areas.Admin.Services.Category.Options;
using Annstore.Web.Infrastructure;
using AutoMapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Factories
{
    public sealed class AdminCategoryService : IAdminCategoryService
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        //*TODO*
        private const string BREADCRUMB_FORMAT = "{0} {1} ";

        public AdminCategoryService(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public async Task<CategoryListModel> GetCategoryListModelAsync(CategoryListOptions options)
        {
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryModels = new List<CategorySimpleModel>();
            foreach (var category in categories)
            {
                var simpleModel = _mapper.Map<CategorySimpleModel>(category);
                if (options.PrepareBreadcrumb)
                {
                    Category targetBreadcrumb = null;
                    if (options.BreadcrumbParentOnly)
                    {
                        var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentId);
                        if (parentCategory != null)
                        {
                            targetBreadcrumb = parentCategory;
                        }
                    }
                    else
                    {
                        targetBreadcrumb = category;
                    }
                    if (targetBreadcrumb != null)
                    {
                        var breadcrumb = await GetCategoryBreadcrumbStringAsync(targetBreadcrumb, options.BreadcrumbDeepLevel, options.BreadcrumbSeparator);
                        simpleModel.Breadcrumb = breadcrumb;
                    }
                }
                categoryModels.Add(simpleModel);
            }

            var model = new CategoryListModel
            {
                Categories = categoryModels
            };

            return model;
        }

        public async Task<CategoryModel> GetCategoryModelAsync(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return null;

            var model = _mapper.Map<CategoryModel>(category);
            await PrepareCategoryModelParentCategoriesAsync(model);

            return model;
        }

        public async Task<AppResponse<Category>> CreateCategoryAsync(AppRequest<CategoryModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var category = _mapper.Map<Category>(request.Data);
                await _categoryService.CreateCategoryAsync(category);
                return AppResponse.SuccessResult(category);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult<Category>(AdminMessages.Category.CreateCategoryError);
            }
        }

        public async Task<CategoryModel> PrepareCategoryModelParentCategoriesAsync(CategoryModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var categories = await _categoryService.GetCategoriesAsync();
            if (model.Id != 0)
            {
                var removeItemIndex = categories.FindIndex(c => c.Id == model.Id);
                if (removeItemIndex >= 0)
                    categories.RemoveAt(removeItemIndex);
            }
            var parentableCategories = categories
                .Select(c => _mapper.Map<CategorySimpleModel>(c))
                .ToList();
            model.ParentableCategories = parentableCategories;

            return model;
        }

        public async Task<AppResponse<Category>> UpdateCategoryAsync(AppRequest<CategoryModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var category = await _categoryService.GetCategoryByIdAsync(request.Data.Id);
            if (category == null)
                return AppResponse.InvalidModelResult<Category>(AdminMessages.Category.CategoryIsNotFound);

            try
            {
                category = _mapper.Map(request.Data, category);
                await _categoryService.UpdateCategoryAsync(category);

                return AppResponse.SuccessResult(category);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult<Category>(AdminMessages.Category.UpdateCategoryError);
            }
        }

        public async Task<AppResponse> DeleteCategoryAsync(AppRequest<int> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var category = await _categoryService.GetCategoryByIdAsync(request.Data);
            if (category == null)
                return AppResponse.InvalidModelResult(AdminMessages.Category.CategoryIsNotFound);

            try
            {
                await _categoryService.DeleteCategoryAsync(category);

                return AppResponse.SuccessResult(AdminMessages.Category.DeleteCategorySuccess);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult(AdminMessages.Category.DeleteCategoryError);
            }
        }

        public async Task<string> GetCategoryBreadcrumbStringAsync(Category category, int deepLevel, string separator)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var breadcrumb = await _categoryService.GetCategoryBreadcrumbAsync(category, deepLevel);
            var breadcrumbStringBuilder = new StringBuilder();
            for (var i = 0; i < breadcrumb.Count; i++)
            {
                var item = breadcrumb[i];
                if (i != breadcrumb.Count - 1)
                {
                    breadcrumbStringBuilder.AppendFormat(BREADCRUMB_FORMAT, item.Name, separator);
                }
                else
                {
                    breadcrumbStringBuilder.Append(item.Name);
                }
            }

            return breadcrumbStringBuilder.ToString();
        }
    }
}
