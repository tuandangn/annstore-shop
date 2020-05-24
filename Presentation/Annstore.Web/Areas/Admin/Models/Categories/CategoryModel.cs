using System;
using System.Collections;
using System.Collections.Generic;

namespace Annstore.Web.Areas.Admin.Models.Categories
{
    [Serializable]
    public sealed class CategoryModel
    {
        public CategoryModel()
        {
            this.ParentableCategories = new List<CategorySimpleModel>();
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
