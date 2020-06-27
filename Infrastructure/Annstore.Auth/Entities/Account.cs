using Microsoft.AspNetCore.Identity;
using System;

namespace Annstore.Auth.Entities
{
    [Serializable]
    public sealed class Account : IdentityUser<int>
    {
        public int CustomerId { get; set; }
    }
}
