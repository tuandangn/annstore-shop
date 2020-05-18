using Annstore.Core.Entities;
using Annstore.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Annstore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AnnstoreDbContext _dbContext;

        public CategoryService(AnnstoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<List<Category>> GetCategories()
        {
            var query = from category in _dbContext.Categories
                        select category;

            var result = await query.ToListAsync();

            return result;
        }
    }
}
