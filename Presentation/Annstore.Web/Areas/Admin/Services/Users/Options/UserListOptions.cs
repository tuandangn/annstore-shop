using System;

namespace Annstore.Web.Areas.Admin.Services.Users.Options
{
    [Serializable]
    public sealed class UserListOptions
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
