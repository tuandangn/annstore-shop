using Microsoft.AspNetCore.Identity;

namespace Annstore.Auth.Entities
{
    public class Account : IdentityUser<int>
    {
        public int CustomerId { get; set; }
    }
}
