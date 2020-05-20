using Annstore.Web.Areas.Admin.Models.Categories;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Factories
{
    public interface ICategoryModelFactory
    {
        Task<CategoryModel> PrepareCategoryModelParentCategories(CategoryModel model);
    }
}
