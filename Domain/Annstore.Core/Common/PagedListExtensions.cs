using System.Collections.Generic;

namespace Annstore.Core.Common
{
    public static class PagedListExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> items, int pageSize, int pageNumber, int totalItems)
        {
            return new PagedList<T>(items, pageSize, pageNumber, totalItems);
        }
    }
}
