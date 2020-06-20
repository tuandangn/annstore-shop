using System;
using System.Collections;
using System.Collections.Generic;

namespace Annstore.Core.Common
{
    [Serializable]
    public sealed class PagedList<T> : IPagedList<T>
    {
        public PagedList(IEnumerable<T> items, int pageSize, int pageNumber, int totalItems)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (pageSize <= 0)
                throw new ArgumentException($"{nameof(pageSize)} cannot less than or equal to 0");
            if (totalItems < 0)
                throw new ArgumentException($"{nameof(totalItems)} cannot less than or equal to 0");

            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);
            TotalItems = totalItems;
            Items = items;
        }

        public int PageSize { get; }

        public int PageNumber { get; }

        public int PageIndex => PageNumber - 1;

        public int TotalPages { get; }

        public int TotalItems { get; }

        public IEnumerable<T> Items { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
