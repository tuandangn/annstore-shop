using System;

namespace Annstore.Application.Models.Customers
{
    [Serializable]
    public sealed class LoginModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool Remember { get; set; }
    }
}
