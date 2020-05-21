using System;

namespace Annstore.Web.Infrastructure
{
    [Serializable]
    public sealed class CategorySettings
    {
        public string AdminBreadcrumbSeparator { get; set; }

        public bool AdminBreadcrumbParentOnly { get; set; }

        public int AdminBreadcrumbDeepLevel { get; set; }
    }
}
