using Annstore.Core.Entities.Catalog;
using AutoMapper;
using QueryCategory = Annstore.Query.Entities.Catalog.Category;

namespace Annstore.DataMixture.Mappings
{
    public sealed class MixDataProfile : Profile
    {
        public MixDataProfile()
        {
            //category
            CreateMap<Category, QueryCategory>()
                .ForMember(model => model.Id, config => config.Ignore())
                .ForMember(model => model.EntityId, config => config.MapFrom(source => source.Id))
                .ForMember(model => model.Children, config => config.Ignore());
        }
    }
}
