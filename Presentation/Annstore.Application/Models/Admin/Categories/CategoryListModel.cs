using System;
using System.Collections.Generic;
using Annstore.Application.Models.Admin.Common;

namespace Annstore.Application.Models.Admin.Categories
{
    [Serializable]
    public sealed class CategoryListModel : PagedModel
    {
        public CategoryListModel()
        {
            Categories = new List<CategorySimpleModel>();
        }

        public IList<CategorySimpleModel> Categories { get; set; }
    }
}
