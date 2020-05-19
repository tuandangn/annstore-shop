﻿using System;
using System.Collections.Generic;
using Annstore.Core.Entities.Catalog;

namespace Annstore.Web.Areas.Admin.Models.Categories
{
    [Serializable]
    public sealed class CategoryListModel
    {
        public CategoryListModel()
        {
            Categories = new List<CategorySimpleModel>();
        }

        public IList<CategorySimpleModel> Categories { get; set; }
    }
}
