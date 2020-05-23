using System;

namespace Annstore.Web.Models.Accounts
{
    [Serializable]
    public sealed class LoginModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool Remember { get; set; }
    }
}
