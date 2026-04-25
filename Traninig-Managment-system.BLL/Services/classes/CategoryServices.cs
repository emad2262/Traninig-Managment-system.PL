
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

        public async Task<CategoryAndCoursesDto> GetCategoryById(int CompanyId, int Categoryid)
        {
            var category = await _categoryRepo.GetOneAsync(
            c => c.CompanyId == CompanyId && c.Id == Categoryid,
            c => c.Courses);

            if (category == null)
            {
                return null!; 
            }

            return new CategoryAndCoursesDto
            {
                Id = category.Id,
                Name = category.Name,

                // 3. التأكد إن الكورسات مش بـ Null قبل ما نلف عليها، وحماية اسم المدرب
                Courses = category.Courses?.Select(course => new CourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    Logo = course.logo,
                }).ToList() ?? new List<CourseDto>() 
            };
        }
    }
}
