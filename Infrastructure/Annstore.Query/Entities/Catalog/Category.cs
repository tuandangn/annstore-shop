using System;
using System.Collections.Generic;

namespace Annstore.Query.Entities.Catalog
{
    [Serializable]
    public sealed class Category : QueryBaseEntity
    {
        private IEnumerable<Category> _children;

        private IEnumerable<Category> _breadcrumb;

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public IEnumerable<Category> Breadcrumb
        {
            get
            {
                if (_breadcrumb == null)
                    _breadcrumb = new List<Category>();
                return _breadcrumb;
            }
            set
            {
                _breadcrumb = value;
            }
        }

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
