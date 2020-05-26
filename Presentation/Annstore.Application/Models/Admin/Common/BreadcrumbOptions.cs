using System;

namespace Annstore.Application.Models.Admin.Common
{
    [Serializable]
    public class BreadcrumbOptions
    {
        public bool Enable { get; set; }

        public string Separator { get; set; }

        public int DeepLevel { get; set; }

        public bool UseParentAsTarget { get; set; }

        public bool ShowHidden { get; set; }
    }
}
