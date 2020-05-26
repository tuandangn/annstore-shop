using Annstore.Application.Models.Customers;
using FluentValidation;

namespace Annstore.Web.Validators.Accounts
{
    public sealed class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(l => l.Email).NotEmpty().WithMessage("Email không để trống");
            RuleFor(l => l.Password).NotEmpty().WithMessage("Mật khẩu không để trống");
        }
    }
}
