using System;

namespace Annstore.Application.Models.Admin.Common
{
    [Serializable]
    public class PagedModel
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
