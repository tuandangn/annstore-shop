using Annstore.Web.Areas.Admin.Models.Common;
using System;
using System.Collections.Generic;

namespace Annstore.Web.Areas.Admin.Models.Users
{
    [Serializable]
    public sealed class UserListModel : PagedModel
    {
        public UserListModel()
        {
            Users = new List<UserSimpleModel>();
        }

        public IList<UserSimpleModel> Users { get; set; }
    }
}
