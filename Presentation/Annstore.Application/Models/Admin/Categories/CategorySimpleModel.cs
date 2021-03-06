﻿using System;

namespace Annstore.Application.Models.Admin.Categories
{
    [Serializable]
    public sealed class CategorySimpleModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public bool Published { get; set; }

        public string Breadcrumb { get; set; }
    }
}
