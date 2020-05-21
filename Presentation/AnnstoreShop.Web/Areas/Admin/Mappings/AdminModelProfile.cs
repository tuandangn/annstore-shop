using Annstore.Core.Entities.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using AutoMapper;

namespace Annstore.Web.Areas.Admin.Mappings
{
    public sealed class AdminModelProfile : Profile
    {
        public AdminModelProfile()
        {
            CreateMap<Category, CategorySimpleModel>()
                .ForMember(model => model.Breadcrumb, config => config.Ignore());
            CreateMap<Category, CategoryModel>()
                .ForMember(model => model.ParentableCategories, config => config.Ignore());
            CreateMap<CategoryModel, Category>();
        }
    }
}
