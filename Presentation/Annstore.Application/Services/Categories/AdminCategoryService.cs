﻿using Annstore.Application.Infrastructure;
using Annstore.Application.Infrastructure.Messages.Messages;
using Annstore.Application.Models.Admin.Categories;
using Annstore.Application.Models.Admin.Common;
using Annstore.Core.Entities.Catalog;
using Annstore.Services.Catalog;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Categories
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
            var pagedCategories = await _categoryService.GetPagedCategoriesAsync(
                options.PageNumber,
                options.PageSize,
                true
            ).ConfigureAwait(false);
            var categoryModels = new List<CategorySimpleModel>();
            foreach (var category in pagedCategories)
            {
                var simpleModel = _mapper.Map<CategorySimpleModel>(category);

                //breadcrumb
                var breadcrumbOpts = options.Breadcrumb;
                if (breadcrumbOpts.Enable)
                {
                    var breadcrumb = await GetCategoryBreadcrumbStringAsync(
                        category,
                        breadcrumbOpts.DeepLevel,
                        breadcrumbOpts.Separator,
                        breadcrumbOpts.UseParentAsTarget,
                        breadcrumbOpts.ShowHidden)
                        .ConfigureAwait(false);
                    simpleModel.Breadcrumb = breadcrumb;
                }
                categoryModels.Add(simpleModel);
            }

            var model = new CategoryListModel
            {
                Categories = categoryModels,
                PageNumber = pagedCategories.PageNumber,
                PageSize = pagedCategories.PageSize,
                TotalItems = pagedCategories.TotalItems,
                TotalPages = pagedCategories.TotalPages
            };

            return model;
        }

        public async Task<CategoryModel> GetCategoryModelAsync(int id, BreadcrumbOptions breadcrumbOpts)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id)
                .ConfigureAwait(false);
            if (category == null || category.Deleted)
                return CategoryModel.NullModel;

            var model = _mapper.Map<CategoryModel>(category);
            await PrepareCategoryModelParentCategoriesAsync(model, breadcrumbOpts, true)
                .ConfigureAwait(false);

            return model;
        }

        public async Task<AppResponse<Category>> CreateCategoryAsync(AppRequest<CategoryModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var category = _mapper.Map<Category>(request.Data);
                await _categoryService.CreateCategoryAsync(category)
                    .ConfigureAwait(false);
                return AppResponse.SuccessResult(category);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult<Category>(AdminMessages.Category.CreateCategoryError);
            }
        }

        public async Task<CategoryModel> PrepareCategoryModelParentCategoriesAsync(CategoryModel model, BreadcrumbOptions breadcrumbOpts, bool showHidden = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var categories = await _categoryService.GetCategoriesAsync(showHidden)
                .ConfigureAwait(false);
            if (model.Id != 0)
            {
                var removeItemIndex = categories.FindIndex(c => c.Id == model.Id);
                if (removeItemIndex >= 0)
                    categories.RemoveAt(removeItemIndex);
            }
            var parentableCategories = new List<CategorySimpleModel>();
            foreach (var category in categories)
            {
                var categoryModel = _mapper.Map<CategorySimpleModel>(category);

                //breadcrumb
                if (breadcrumbOpts.Enable)
                {
                    var breadcrumb = await GetCategoryBreadcrumbStringAsync(
                        category, breadcrumbOpts.DeepLevel,
                        breadcrumbOpts.Separator, breadcrumbOpts.UseParentAsTarget,
                        breadcrumbOpts.ShowHidden)
                       .ConfigureAwait(false);
                    categoryModel.Breadcrumb = breadcrumb;
                }

                parentableCategories.Add(categoryModel);
            }
            model.ParentableCategories = parentableCategories;

            return model;
        }

        public async Task<AppResponse<Category>> UpdateCategoryAsync(AppRequest<CategoryModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var category = await _categoryService.GetCategoryByIdAsync(request.Data.Id)
                .ConfigureAwait(false);
            if (category == null || category.Deleted)
                return AppResponse.InvalidModelResult<Category>(AdminMessages.Category.CategoryIsNotFound);

            try
            {
                category = _mapper.Map(request.Data, category);
                await _categoryService.UpdateCategoryAsync(category)
                    .ConfigureAwait(false);

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

            var category = await _categoryService.GetCategoryByIdAsync(request.Data)
                .ConfigureAwait(false);
            if (category == null)
                return AppResponse.InvalidModelResult(AdminMessages.Category.CategoryIsNotFound);

            try
            {
                if (!category.Deleted)
                {
                    await _categoryService.DeleteCategoryAsync(category)
                        .ConfigureAwait(false);
                }

                return AppResponse.SuccessResult(AdminMessages.Category.DeleteCategorySuccess);
            }
            catch (Exception ex)
            {
                //*TODO*
                return AppResponse.ErrorResult(AdminMessages.Category.DeleteCategoryError);
            }
        }

        public async Task<string> GetCategoryBreadcrumbStringAsync(Category category, int deepLevel, string separator, bool useParentAsTarget, bool showHidden = false)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var target = category;
            if (useParentAsTarget)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentId)
                    .ConfigureAwait(false);
                if (parentCategory != null && !parentCategory.Deleted && (showHidden || parentCategory.Published))
                {
                    target = parentCategory;
                }
                else
                {
                    return string.Empty;
                }
            }

            var breadcrumb = await _categoryService.GetCategoryBreadcrumbAsync(target, deepLevel, showHidden)
                .ConfigureAwait(false);
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
