using Annstore.Services.Catalog;
using Annstore.Web.Areas.Admin.Models.Categories;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;

namespace Annstore.Web.Areas.Admin.Factories
{
    public sealed class CategoryModelFactory : ICategoryModelFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryModelFactory(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public async Task<CategoryModel> PrepareCategoryModelParentCategories(CategoryModel model)
        {
            var categories = await _categoryService.GetCategoriesAsync();

            if(model.Id != 0)
            {
                var removeItemIndex = categories.FindIndex(c => c.Id == model.Id);
                if(removeItemIndex >= 0)
                    categories.RemoveAt(removeItemIndex);
            }

            var parentableCategories = categories
                .Select(c => _mapper.Map<CategorySimpleModel>(c))
                .ToList();

            model.ParentableCategories = parentableCategories;
            return model;
        }
    }
}
