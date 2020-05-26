using System;

namespace Annstore.Application.Infrastructure.Settings
{
    [Serializable]
    public sealed class AccountSettings
    {
        public AccountSettings()
        {
            Admin = new AdminAccountSettings();
        }

        public AdminAccountSettings Admin { get; set; }

        [Serializable]
        public sealed class AdminAccountSettings
        {
            public int DefaultPageSize { get; set; }
        }
    }
}
