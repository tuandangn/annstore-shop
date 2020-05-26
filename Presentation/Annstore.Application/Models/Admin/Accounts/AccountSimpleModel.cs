using System;

namespace Annstore.Application.Models.Admin.Accounts
{
    [Serializable]
    public sealed class AccountSimpleModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Customer { get; set; }
    }
}
