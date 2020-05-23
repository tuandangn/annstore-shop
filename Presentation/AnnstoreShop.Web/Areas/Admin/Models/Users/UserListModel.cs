using System;
using System.Collections.Generic;

namespace Annstore.Web.Areas.Admin.Models.Users
{
    [Serializable]
    public sealed class UserListModel
    {
        public UserListModel()
        {
            Users = new List<UserSimpleModel>();
        }

        public IList<UserSimpleModel> Users { get; set; }
    }
}
