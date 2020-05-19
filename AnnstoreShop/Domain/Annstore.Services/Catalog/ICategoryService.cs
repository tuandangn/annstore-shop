using System.Collections.Generic;
using System.Threading.Tasks;
using Annstore.Core.Entities.Catalog;

namespace Annstore.Services.Catalog
{
    public interface ICategoryService
    {
        ValueTask<List<Category>> GetCategories();
    }
}
