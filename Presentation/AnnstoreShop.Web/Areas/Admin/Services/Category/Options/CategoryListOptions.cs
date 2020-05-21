using System;

namespace Annstore.Web.Areas.Admin.Services.Category.Options
{
    [Serializable]
    public struct CategoryListOptions
    {
        public bool PrepareBreadcrumb { get; set; }

        public string BreadcrumbSeparator { get; set; }

        public int BreadcrumbDeepLevel { get; set; }

        public bool BreadcrumbParentOnly { get; set; }
    }
}
