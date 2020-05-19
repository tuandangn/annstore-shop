using Annstore.Core.Entities.Catalog;
using AnnstoreShop.Web.Areas.Admin.Models.Categories;
using AutoMapper;

namespace AnnstoreShop.Web.Areas.Admin.Mappings
{
    public sealed class AdminModelProfile : Profile
    {
        public AdminModelProfile()
        {
            CreateMap<Category, CategorySimpleModel>();
            CreateMap<Category, CategoryModel>();            
            CreateMap<CategoryModel, Category>();
        }
    }
}
