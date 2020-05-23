using System;
using System.Collections.Generic;
using Annstore.Core.Entities.Catalog;
using Annstore.Web.Areas.Admin.Models.Common;

namespace Annstore.Web.Areas.Admin.Models.Categories
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
