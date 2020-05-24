using System;

namespace Annstore.Core.Entities.Catalog
{
    [Serializable]
    public sealed class Category : BaseEntity
    {
        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ParentId { get; set; }

        public string Description { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }
    }
}
