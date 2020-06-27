using Microsoft.AspNetCore.Identity;
using System;

namespace Annstore.Auth.Entities
{
    [Serializable]
    public sealed class Role : IdentityRole<int>
    {
    }
}
