using System;

namespace Annstore.Application.Models.Admin.Common
{
    [Serializable]
    public class CategoryListOptions
    {
        public CategoryListOptions()
        {
            Breadcrumb = new BreadcrumbOptions();
        }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public BreadcrumbOptions Breadcrumb { get; set; }
    }
}
