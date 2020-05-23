using System;

namespace Annstore.Web.Areas.Admin.Models.Users
{
    [Serializable]
    public sealed class UserModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
