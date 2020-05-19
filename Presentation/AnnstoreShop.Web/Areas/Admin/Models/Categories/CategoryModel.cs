using System;

namespace Annstore.Web.Areas.Admin.Models.Categories
{
    [Serializable]
    public sealed class CategoryModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ParentId { get; set; }
    }
}
