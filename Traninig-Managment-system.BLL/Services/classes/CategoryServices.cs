using Traninig_Managment_system.BLL.ModelVm;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CategoryServices : ICategoryServices
    {
        private readonly ICategoryRepo _categoryCourses;

        public CategoryServices(ICategoryRepo categoryCourses)
        {
            _categoryCourses = categoryCourses;
        }

       

        public async Task<IEnumerable<CategoryVm>> GetCategoriesInCompany(int CompanyId)
        {
            var getcategories = await _categoryCourses.GetAllCategory(CompanyId);

            var categories = getcategories.Select(e => new CategoryVm
            {
                Id=e.Id,
                CategoryName = e.Name,
                Courses = e.Courses.Select(c => new CourseVm
                {
                    Id = c.Id,
                    CourseName = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor != null
                    ? c.Instructor.FullName
                    : "Not Assigned Yet",
                    logoUrl = c.logo,
                }).ToList()
            }).ToList();

            return categories;
        }

        public async Task AddCategories(int CompanyId, CreateCategoryVm categoryVm)
        {
            var categories = await _categoryCourses.GetAllCategory(CompanyId);

            var category = new CourseCategory
            {
                Name=categoryVm.CategoryName,
                CompanyId = CompanyId,
                
            };
            await _categoryCourses.CreateAsync(category);   
        }

        public async Task DeleteCategories(int CompanyId, int categoryId)
        {
            var category = await _categoryCourses.GetOneAsync(
                e => e.Id == categoryId && e.CompanyId == CompanyId
            );

            if (category == null)
                throw new Exception("Category not found or not authorized");

            await _categoryCourses.Delete(category);
        }
        /// <summary>
        /// display instructor
        /// </summary>


        public async Task<IEnumerable<CategoryVm>> GetCategoriesForInstructorAsync(int companyId, string instructorUserId)
        {
            var categories = await _categoryCourses.GetCategoriesForInstructorAsync(companyId, instructorUserId);

            return categories
                .Select(c => new CategoryVm
                {
                    Id = c.Id,
                    CategoryName = c.Name,
                    Courses = c.Courses.Select(course => new CourseVm
                    {
                        Id=course.Id,
                        CourseName = course.Title,
                        Description = course.Description,
                        InstructorName = course.Instructor.FullName,
                        logoUrl = course.logo
                    }).ToList()
                })
                .Where(c => c.Courses.Any())
                .ToList();
        }
    }
}


