using System.Collections.Generic;

namespace Annstore.Query.Entities.Catalog
{
    public sealed class Category : QueryBaseEntity
    {
        private IEnumerable<Category> _children;

        public string Name { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public IEnumerable<Category> Children
        {
            get
            {
                if (_children == null)
                    _children = new List<Category>();
                return _children;
            }
            set
            {
                _children = value;
            }
        }
    }
}
