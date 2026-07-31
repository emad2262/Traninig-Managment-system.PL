

using Traninig_Managment_system.BLL.Dtos.Course;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }
        public async Task<int> CategoryCount(int companyId)
        {
           var categorycount=await _categoryRepo.CountAsync(e=>e.CompanyId==companyId);
            return categorycount;
        }


        public async Task<IEnumerable<CategoryListDto>> GetAllCategoyr(int companyId)
        {
            var categories = await _categoryRepo.GetAllAsync(e => e.CompanyId == companyId);


            return  categories
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                CategoryName = c.Name,
                CompanyId = c.CompanyId,
                TotalCourses = c.Courses?.Count ?? 0,
                
            });  
        }
        public async Task<CategoryDetailsDto?> GetCategoryByIdAsync(int companyId, int categoryId)
        {
            var category = await _categoryRepo.GetOneAsync(e=>e.Id==categoryId && e.CompanyId == companyId);

            if (category == null)
            {
                return null;
            }
            return new CategoryDetailsDto
            {

                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryDescription = category.Description,

                CompanyId = category.CompanyId,
                CreatedAt = category.CreatedAt,
                TotalCourse = category.Courses?.Count ?? 0,
                CourseListDtos = category.Courses.Select(e => new ListCourseDto
                {
                    Id=e.Id,
                    Title=e.Title,
                    logo=e.logo,
                    DurationInHours=e.DurationInHours,
                    IsPublished=e.IsPublished,
                   

                }).ToList()
            };
        }


        public async Task<int> CreateCategoryAsync(CreateCategoryDto model, int companyId)
        {
            var categoryEntity = new Category
            {
                Name = model.Name,
                Description=model.Description??"",
                CompanyId = companyId

            };
            await _categoryRepo.CreateAsync(categoryEntity);
            await _categoryRepo.SaveChangesAsync();
            return categoryEntity.Id;
        }

        public async Task UpdateCategoryAsync(int companyId, UpdateCategoryDto dto)
        {
            var category = await _categoryRepo.GetOneAsync(c =>
                c.CompanyId == companyId &&
                c.Id == dto.Id);

            if (category == null)
            {
                throw new InvalidOperationException("Category was not found.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description ?? "";

            await _categoryRepo.Update(category);
            await _categoryRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId, int companyId)
        {
            var category = await _categoryRepo.GetOneAsync(
                c => c.Id == categoryId && c.CompanyId == companyId);

            if (category is  null)
            {
                return false;
            }
            if (category.Courses != null && category.Courses.Any())
            {
                throw new InvalidOperationException("The section cannot be deleted because it contains related courses..");
            }

            await _categoryRepo.Delete(category);
            await _categoryRepo.SaveChangesAsync();
            return true;

        }


    }
}
