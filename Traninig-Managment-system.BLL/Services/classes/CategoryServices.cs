
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<bool> CreateCategoryAsync(CreateCategoryVM model)
        {
            var existingCategory = await _categoryRepo.GetOneAsync(
               c => c.CompanyId == model.CompanyId && c.Name == model.Name);

            if (existingCategory != null)
            {
                return false;
            }

            var newCategory = new Category
            {
                Name = model.Name,
                CompanyId = model.CompanyId
            };

            return await _categoryRepo.CreateAsync(newCategory);
        }

        public async Task<IEnumerable<CategoryDisplayVM>> GetCategoriesByCompanyAsync(int companyId)
        {
            var categories = await _categoryRepo.GetAllAsync(
                 c => c.CompanyId == companyId,
                 c => c.Courses
            );

            var categoryVMs = categories.Select(c => new CategoryDisplayVM
            {
                Id = c.Id,
                Name = c.Name,
                CompanyId = c.CompanyId,
                TotalCourses = c.Courses != null ? c.Courses.Count : 0
            });

            return categoryVMs;
        }
    }
}
