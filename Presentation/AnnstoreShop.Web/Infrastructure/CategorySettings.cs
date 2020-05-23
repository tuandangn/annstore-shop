using System;

namespace Annstore.Web.Infrastructure
{
    [Serializable]
    public sealed class CategorySettings
    {
        public CategorySettings()
        {
            Admin = new AdminCategorySettings();
        }

        public AdminCategorySettings Admin { get; set; }

        [Serializable]
        public sealed class AdminCategorySettings
        {
            public AdminCategorySettings()
            {
                Breadcrumb = new BreadcrumbSettings();
            }

            public BreadcrumbSettings Breadcrumb { get; set; }
        }

        [Serializable]
        public sealed class BreadcrumbSettings
        {
            public bool Enable { get; set; }

            public string Separator { get; set; }

            public bool UseParentAsTarget { get; set; }

            public int DeepLevel { get; set; }
        }
    }
}
