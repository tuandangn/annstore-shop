using Annstore.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annstore.Services
{
    public interface ICategoryService
    {
        ValueTask<List<Category>> GetCategories();
    }
}
