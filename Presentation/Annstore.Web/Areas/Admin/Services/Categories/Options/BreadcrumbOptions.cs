using System;

namespace Annstore.Web.Areas.Admin.Services.Categories.Options
{
    [Serializable]
    public class BreadcrumbOptions
    {
        public bool Enable { get; set; }

        public string Separator { get; set; }

        public int DeepLevel { get; set; }

        public bool UseParentAsTarget { get; set; }
    }
}
