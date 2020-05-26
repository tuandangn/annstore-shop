using System;

namespace Annstore.Application.Infrastructure.Settings
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

            public int DefaultPageSize { get; set; }

            public BreadcrumbSettings Breadcrumb { get; set; }
        }

        [Serializable]
        public sealed class BreadcrumbSettings
        {
            public bool Enable { get; set; }

            public string Separator { get; set; }

            public int DeepLevel { get; set; }

            public bool UseParentAsTarget { get; set; }

            public bool ShowHidden { get; set; }
        }
    }
}
