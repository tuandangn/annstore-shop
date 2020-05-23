using System;

namespace Annstore.Web.Areas.Admin.Models.Users
{
    [Serializable]
    public sealed class UserSimpleModel
    {
        public int Id { get; set; }

        public string Email { get; set; }
    }
}
