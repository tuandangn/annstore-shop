using Annstore.Application.Models.Admin.Customers;
using System;

namespace Annstore.Application.Models.Admin.Accounts
{
    [Serializable]
    public sealed class AccountSimpleModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public CustomerSimpleModel Customer { get; set; }
    }
}
