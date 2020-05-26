using Annstore.Application.Models.Admin.Common;
using System;
using System.Collections.Generic;

namespace Annstore.Application.Models.Admin.Customers
{
    [Serializable]
    public sealed class CustomerListModel : PagedModel
    {
        public CustomerListModel()
        {
            Customers = new List<CustomerSimpleModel>();
        }

        public IList<CustomerSimpleModel> Customers { get; set; }
    }
}
