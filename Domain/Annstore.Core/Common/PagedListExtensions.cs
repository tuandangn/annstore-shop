using System.Collections.Generic;

namespace Annstore.Core.Common
{
    public static class PagedListExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageSize, int pageNumber, int totalItems)
        {
            return new PagedList<T>(source, pageSize, pageNumber, totalItems);
        }

        public static IPagedList<T> ToPagedList<T>(this IList<T> source, int pageSize, int pageNumber, int totalItems)
        {
            return new PagedList<T>(source, pageSize, pageNumber, totalItems);
        }
    }
}
