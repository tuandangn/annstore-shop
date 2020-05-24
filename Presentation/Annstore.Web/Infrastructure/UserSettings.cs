using System;

namespace Annstore.Web.Infrastructure
{
    [Serializable]
    public sealed class UserSettings
    {
        public UserSettings()
        {
            Admin = new AdminUserSettings();
        }

        public AdminUserSettings Admin { get; set; }

        [Serializable]
        public sealed class AdminUserSettings
        {
            public int DefaultPageSize { get; set; }
        }
    }
}
