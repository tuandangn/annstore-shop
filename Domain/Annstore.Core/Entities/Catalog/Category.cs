using System;

namespace Annstore.Core.Entities.Catalog
{
    [Serializable]
    public sealed class Category : BaseEntity, IAggregateRoot, IHideableEntity
    {
        private bool _deleted;

        public Category() : this(0, false) { }

        private Category(int id) : this(id, false) { }

        private Category(int id, bool deleted) : base(id)
        {
            _deleted = deleted;
        }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ParentId { get; set; }

        public string Description { get; set; }

        public bool Published { get; set; }

        public bool Deleted
        {
            get => _deleted;
            private set => _deleted = value;
        }

        public static Category CreateWithId(int id) => new Category(id);

        public void IsDeleted(bool deleted) => Deleted = deleted;
    }
}
