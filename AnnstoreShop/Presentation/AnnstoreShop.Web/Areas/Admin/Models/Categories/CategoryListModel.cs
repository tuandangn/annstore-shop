using Annstore.Core.Entities;
using System.Collections;
using System.Collections.Generic;

namespace AnnstoreShop.Web.Areas.Admin.Models.Categories
{
    public class CategoryListModel
    {
        public CategoryListModel()
        {
            Categories = new List<Category>();
        }

        public IList<Category> Categories { get; set; }
    }
}
