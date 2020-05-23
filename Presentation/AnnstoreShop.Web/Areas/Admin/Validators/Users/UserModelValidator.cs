using Annstore.Web.Areas.Admin.Models.Users;
using FluentValidation;

namespace Annstore.Web.Areas.Admin.Validators.Users
{
    public sealed class UserModelValidator : AbstractValidator<UserModel>
    {
        public UserModelValidator()
        {
            RuleFor(p => p.Email).NotEmpty().WithMessage("Email không để trống");
            RuleFor(p => p.Email).EmailAddress().WithMessage("Email không đúng");

            When(p => p.Id == 0, () =>
            {
                RuleFor(p => p.Password).NotEmpty().WithMessage("Mật khẩu không để trống");
                RuleFor(p => p.Password).MinimumLength(6).WithMessage("Mật khẩu có ít nhất 6 kí tự");
            });
        }
    }
}
