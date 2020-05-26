using Annstore.Application.Models.Admin.Customers;
using System;
using System.Collections.Generic;

namespace Annstore.Application.Models.Admin.Accounts
{
    [Serializable]
    public sealed class AccountModel
    {
        public AccountModel()
        {
            Customers = new List<CustomerSimpleModel>();
        }

        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int CustomerId { get; set; }

        public IList<CustomerSimpleModel> Customers { get; set; }
    }
}
