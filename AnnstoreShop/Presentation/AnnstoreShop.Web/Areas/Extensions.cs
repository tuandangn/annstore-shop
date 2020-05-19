using Annstore.Core.Entities.Catalog;
using AnnstoreShop.Web.Areas.Admin.Models.Categories;

namespace AnnstoreShop.Web.Areas
{
    public static class ModelExtensions
    {
        public static CategorySimpleModel ToSimpleModel(this Category category)
        {
            var model = new CategorySimpleModel
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder
            };
            return model;
        }
    }
}
