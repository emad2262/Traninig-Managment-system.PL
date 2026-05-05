using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly ApplicationDbContext _context;

        public CategoryService(ICategoryRepo categoryRepo, ApplicationDbContext context)
        {
            _categoryRepo = categoryRepo;
            _context = context;
        }

        public async Task<IEnumerable<CategoryDisplayVM>> GetCategoriesByCompanyAsync(int companyId)
        {
            var today = DateTime.Today;

            return await _context.CourseCategories
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId)
                .Select(c => new CategoryDisplayVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    CompanyId = c.CompanyId,
                    TotalCourses = c.Courses.Count,
                    PublishedCourses = c.Courses.Count(course => course.IsPublished),
                    DraftCourses = c.Courses.Count(course => !course.IsPublished),
                    NextCourseDate = c.Courses
                        .Where(course => course.StartDate >= today)
                        .OrderBy(course => course.StartDate)
                        .Select(course => (DateTime?)course.StartDate)
                        .FirstOrDefault()
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryAndCoursesDto?> GetCategoryByIdAsync(int companyId, int categoryId)
        {
            var category = await _context.CourseCategories
                .AsNoTracking()
                .Include(c => c.Courses)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == categoryId);

            if (category == null)
            {
                return null;
            }

            var courses = category.Courses
                .Select(MapCourseDto)
                .OrderBy(c => c.StartDate)
                .ThenBy(c => c.Title)
                .ToList();

            return new CategoryAndCoursesDto
            {
                Id = category.Id,
                Name = category.Name,
                CompanyId = category.CompanyId,
                TotalCourses = courses.Count,
                PublishedCourses = courses.Count(c => c.IsPublished),
                DraftCourses = courses.Count(c => !c.IsPublished),
                Courses = courses
            };
        }

        public async Task<CreateCategoryVM?> GetCategoryForEditAsync(int companyId, int categoryId)
        {
            var category = await _categoryRepo.GetOneAsync(c => c.CompanyId == companyId && c.Id == categoryId);
            if (category == null)
            {
                return null;
            }

            return new CreateCategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                CompanyId = category.CompanyId
            };
        }

        public async Task<ServiceResult<int>> CreateCategoryAsync(CreateCategoryVM model, int companyId)
        {
            var validation = await ValidateCategoryAsync(model, companyId);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = validation.Message };
            }

            var category = new Category
            {
                Name = NormalizeName(model.Name),
                CompanyId = companyId
            };

            var saved = await _categoryRepo.CreateAsync(category);
            return saved
                ? new ServiceResult<int> { IsSuccess = true, Data = category.Id, Message = "تم إنشاء القسم بنجاح." }
                : new ServiceResult<int> { IsSuccess = false, Message = "حدث خطأ أثناء حفظ القسم." };
        }

        public async Task<ServiceResult<bool>> UpdateCategoryAsync(CreateCategoryVM model, int companyId)
        {
            var category = await _categoryRepo.GetOneAsync(c => c.Id == model.Id && c.CompanyId == companyId);
            if (category == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "القسم غير موجود أو لا يتبع شركتك."
                };
            }

            var validation = await ValidateCategoryAsync(model, companyId);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = validation.Message };
            }

            category.Name = NormalizeName(model.Name);

            var updated = await _categoryRepo.UpdateAsync(category);
            return updated
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "تم تعديل القسم بنجاح." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "حدث خطأ أثناء تعديل القسم." };
        }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(int categoryId, int companyId)
        {
            var category = await _categoryRepo.GetOneAsync(
                c => c.Id == categoryId && c.CompanyId == companyId,
                c => c.Courses);

            if (category == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "القسم غير موجود." };
            }

            if (category.Courses.Any())
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "لا يمكن حذف القسم لأنه يحتوي على كورسات. احذف أو انقل الكورسات أولا."
                };
            }

            var deleted = await _categoryRepo.Delete(category);
            return deleted
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "تم حذف القسم بنجاح." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "فشل حذف القسم." };
        }

        private async Task<ServiceResult<bool>> ValidateCategoryAsync(CreateCategoryVM model, int companyId)
        {
            var name = NormalizeName(model.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "اسم القسم مطلوب." };
            }

            if (name.Length < 2 || name.Length > 100)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "اسم القسم يجب أن يكون بين 2 و 100 حرف." };
            }

            var normalizedName = name.ToLower();
            var duplicateExists = await _context.CourseCategories
                .AsNoTracking()
                .AnyAsync(c =>
                    c.CompanyId == companyId &&
                    c.Id != model.Id &&
                    c.Name.ToLower() == normalizedName);

            if (duplicateExists)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "يوجد قسم بهذا الاسم بالفعل." };
            }

            return new ServiceResult<bool> { IsSuccess = true, Data = true };
        }

        private static string NormalizeName(string? name)
        {
            return string.Join(" ", (name ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static CourseDto MapCourseDto(Course course)
        {
            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = string.IsNullOrWhiteSpace(course.logo) ? null : course.logo,
                DurationInHours = course.DurationInHours,
                IsPublished = course.IsPublished,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId,
                CategoryName = course.Category?.Name,
                InstructorName = course.Instructor?.FullName
            };
        }
    }
}
