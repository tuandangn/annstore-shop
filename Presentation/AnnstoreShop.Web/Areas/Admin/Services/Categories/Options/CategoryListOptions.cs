using System;

namespace Annstore.Web.Areas.Admin.Services.Categories.Options
{
    [Serializable]
    public class CategoryListOptions
    {
        public CategoryListOptions()
        {
            Breadcrumb = new BreadcrumbOptions();
        }

        public BreadcrumbOptions Breadcrumb { get; set; }
    }
}
