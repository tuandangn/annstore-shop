using System.Collections.Generic;

namespace Annstore.Core.Common
{
    public interface IPagedList<out T>
    {
        int PageSize { get; }

        int PageNumber { get; }

        int PageIndex { get; }

        int TotalPages { get; }

        int TotalItems { get; }

        IEnumerable<T> Items { get; }
    }
}
