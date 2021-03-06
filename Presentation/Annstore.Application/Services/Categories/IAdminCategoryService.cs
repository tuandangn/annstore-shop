﻿using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Admin.Categories;
using Annstore.Application.Models.Admin.Common;
using Annstore.Core.Entities.Catalog;
using System.Threading.Tasks;

namespace Annstore.Application.Services.Categories
{
    public interface IAdminCategoryService
    {
        Task<CategoryModel> GetCategoryModelAsync(int id, BreadcrumbOptions breadcrumbOpts);

        Task<CategoryListModel> GetCategoryListModelAsync(CategoryListOptions options);

        Task<CategoryModel> PrepareCategoryModelParentCategoriesAsync(CategoryModel model, BreadcrumbOptions breadcrumbOpts, bool showHidden = false);

        Task<AppResponse<Category>> CreateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse<Category>> UpdateCategoryAsync(AppRequest<CategoryModel> request);

        Task<AppResponse> DeleteCategoryAsync(AppRequest<int> request);

        Task<string> GetCategoryBreadcrumbStringAsync(Category category, int deepLevel, string separator, bool useParenAsTarget, bool showHidden = false);
    }
}
