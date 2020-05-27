using Annstore.Application.Models.Admin.Common;
using System;

namespace Annstore.Application.Models.Admin.Customers
{
    [Serializable]
    public class CustomerModel : NullableModel<NullCustomerModel>
    {
        public int Id { get; set; }

        public string FullName { get; set; }
    }
}
