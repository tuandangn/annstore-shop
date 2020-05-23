using Annstore.Core.Entities.Catalog;
using Annstore.Core.Entities.Users;
using Annstore.Web.Areas.Admin.Models.Categories;
using Annstore.Web.Areas.Admin.Models.Users;
using AutoMapper;

namespace Annstore.Web.Areas.Admin.Mappings
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
            CreateMap<CategoryModel, Category>();

            //user
            CreateMap<AppUser, UserSimpleModel>();
        }
    }
}
