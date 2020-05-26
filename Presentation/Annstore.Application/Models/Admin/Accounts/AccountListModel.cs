using Annstore.Application.Models.Admin.Common;
using System;
using System.Collections.Generic;

namespace Annstore.Application.Models.Admin.Accounts
{
    [Serializable]
    public sealed class AccountListModel : PagedModel
    {
        public AccountListModel()
        {
            Accounts = new List<AccountSimpleModel>();
        }

        public IList<AccountSimpleModel> Accounts { get; set; }
    }
}
