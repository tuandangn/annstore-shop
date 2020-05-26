﻿using System;

namespace Annstore.Application.Models.Admin.Customers
{
    [Serializable]
    public sealed class CustomerModel
    {
        public int Id { get; set; }

        public string FullName { get; set; }
    }
}
