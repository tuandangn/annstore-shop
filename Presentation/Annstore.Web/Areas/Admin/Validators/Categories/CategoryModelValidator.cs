﻿using Annstore.Application.Models.Admin.Categories;
using FluentValidation;

namespace Annstore.Web.Areas.Admin.Validators.Categories
{
    public sealed class CategoryModelValidator : AbstractValidator<CategoryModel>
    {
        public CategoryModelValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Tên danh mục không để trống");
            RuleFor(p => p.Name).MaximumLength(200).WithMessage("Tên danh mục tối đa 400 kí tự");
        }
    }
}
