using System;

namespace Annstore.Web.Areas.Admin.Services.Category.Options
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
