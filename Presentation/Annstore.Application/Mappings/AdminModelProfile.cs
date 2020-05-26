using Annstore.Application.Models.Admin.Accounts;
using Annstore.Application.Models.Admin.Categories;
using Annstore.Application.Models.Admin.Customers;
using Annstore.Auth.Entities;
using Annstore.Core.Entities.Catalog;
using Annstore.Core.Entities.Customers;
using AutoMapper;

namespace Annstore.Application.Mappings
{
    public sealed class AdminModelProfile : Profile
    {
        public AdminModelProfile()
        {
            //category
            CreateMap<Category, CategorySimpleModel>()
                .ForMember(model => model.Breadcrumb, config => config.Ignore());
            CreateMap<Category, CategoryModel>()
                .ForMember(model => model.ParentableCategories, config => config.Ignore());
            CreateMap<CategoryModel, Category>()
                .ForMember(dest => dest.Deleted, config => config.Ignore());

            //account
            CreateMap<Account, AccountSimpleModel>()
                .ForMember(dest => dest.Customer, config => config.Ignore());
            CreateMap<Account, AccountModel>()
                .ForMember(dest => dest.Customers, config => config.Ignore());
            CreateMap<AccountModel, Account>()
                .ForMember(dest => dest.UserName, config => config.MapFrom(model => model.Email));

            //customer
            CreateMap<Customer, CustomerSimpleModel>();
            CreateMap<Customer, CustomerModel>();
            CreateMap<CustomerModel, Customer>()
                .ForMember(dest => dest.Deleted, config => config.Ignore());
        }
    }
}
