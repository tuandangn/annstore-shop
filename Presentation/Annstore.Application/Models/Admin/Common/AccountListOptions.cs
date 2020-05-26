using System;

namespace Annstore.Application.Models.Admin.Common
{
    [Serializable]
    public sealed class AccountListOptions
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
