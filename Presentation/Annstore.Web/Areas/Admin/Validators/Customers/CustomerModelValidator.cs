using Annstore.Application.Models.Admin.Customers;
using FluentValidation;

namespace Annstore.Web.Areas.Admin.Validators.Categories
{
    public sealed class CustomerModelValidator : AbstractValidator<CustomerModel>
    {
        public CustomerModelValidator()
        {
        }
    }
}
