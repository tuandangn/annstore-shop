using Annstore.Application.Models.Admin.Common;
using System;
using System.Collections.Generic;

namespace Annstore.Application.Models.Admin.Categories
{
    [Serializable]
    public class CategoryModel : NullableModel<NullCategoryModel>
    {
        public CategoryModel()
        {
            ParentableCategories = new List<CategorySimpleModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ParentId { get; set; }

        public bool Published { get; set; }

        public string Description { get; set; }

        public IList<CategorySimpleModel> ParentableCategories { get; set; }
    }
}
