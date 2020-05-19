using System;

namespace Annstore.Core.Entities.Catalog
{
    [Serializable]
    public sealed class Category : BaseEntity
    {
        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ParentId { get; set; }
    }
}
