using System;
using System.Collections.Generic;
using Annstore.Core.Entities.Catalog;

namespace AnnstoreShop.Web.Areas.Admin.Models.Categories
{
    [Serializable]
    public sealed class CategoryListModel
    {
        public CategoryListModel()
        {
            Categories = new List<Category>();
        }

        public IList<Category> Categories { get; set; }
    }
}
