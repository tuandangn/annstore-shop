using System;

namespace Annstore.Application.Infrastructure.Settings
{
    [Serializable]
    public sealed class CustomerSettings
    {
        public CustomerSettings()
        {
            Admin = new AdminCustomerSettings();
        }

        public AdminCustomerSettings Admin { get; set; }

        [Serializable]
        public sealed class AdminCustomerSettings
        {
            public int DefaultPageSize { get; set; }
        }
    }
}
