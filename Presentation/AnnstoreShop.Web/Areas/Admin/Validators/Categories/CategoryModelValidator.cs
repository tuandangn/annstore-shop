using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using FluentValidation;

namespace AnnstoreShop.Web.Areas.Admin.Validators.Categories
{
    public class CategoryModelValidator : AbstractValidator<CategoryModel>
    {
        public CategoryModelValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Tên danh mục không để trống");
            RuleFor(p => p.Name).MaximumLength(200).WithMessage("Tên danh mục tối đa 400 kí tự");
        }
    }
}
