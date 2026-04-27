using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepo _courseRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IInstructorRepo _instructorRepo;
        private readonly ApplicationDbContext _context;

        public CourseServices(
            ICourseRepo courseRepo,
            ICategoryRepo categoryRepo,
            IInstructorRepo instructorRepo,
            ApplicationDbContext context)
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _instructorRepo = instructorRepo;
            _context = context;
        }

        public async Task<IEnumerable<CourseDto>> GetAllInCategoryAsync(int companyId, int categoryId)
        {
            var courses = await _courseRepo.GetAllAsync(
                c => c.CategoryId == categoryId && c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            return courses.Select(MapCourseDto).OrderBy(c => c.StartDate).ThenBy(c => c.Title).ToList();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesAsync(int companyId)
        {
            var courses = await _courseRepo.GetAllAsync(
                c => c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            return courses.Select(MapCourseDto).OrderBy(c => c.CategoryName).ThenBy(c => c.Title).ToList();
        }

        public async Task<CourseDto?> GetByIdAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == id && c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            return course == null ? null : MapCourseDto(course);
        }

        public async Task<ServiceResult<int>> CreateCourseAsync(CourseDto dto, int companyId)
        {
            var validation = await ValidateCourseAsync(dto, companyId);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = validation.Message };
            }

            var course = new Course
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                DurationInHours = dto.DurationInHours,
                IsPublished = false,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                logo = dto.Logo ?? string.Empty,
                CategoryId = dto.CategoryId,
                InstructorId = dto.InstructorId
            };

            var saved = await _courseRepo.CreateAsync(course);
            return saved
                ? new ServiceResult<int> { IsSuccess = true, Data = course.Id, Message = "تم إنشاء الكورس بنجاح." }
                : new ServiceResult<int> { IsSuccess = false, Message = "حدث خطأ أثناء حفظ الكورس." };
        }

        public async Task<ServiceResult<bool>> UpdateAsync(CourseDto dto, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(c => c.Id == dto.Id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "الكورس غير موجود أو لا يتبع شركتك."
                };
            }

            var validation = await ValidateCourseAsync(dto, companyId);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = validation.Message };
            }

            course.Title = dto.Title.Trim();
            course.Description = dto.Description.Trim();
            course.DurationInHours = dto.DurationInHours;
            course.StartDate = dto.StartDate;
            course.EndDate = dto.EndDate;
            course.CategoryId = dto.CategoryId;
            course.InstructorId = dto.InstructorId;

            if (!string.IsNullOrWhiteSpace(dto.Logo))
            {
                course.logo = dto.Logo;
            }

            var updated = await _courseRepo.UpdateAsync(course);
            return updated
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "تم تعديل الكورس بنجاح." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "حدث خطأ أثناء تعديل الكورس." };
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(c => c.Id == id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "الكورس غير موجود." };
            }

            if (await _context.EmployeeCourses.AnyAsync(ec => ec.CourseId == id))
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "لا يمكن حذف الكورس لأنه مرتبط بموظفين."
                };
            }

            if (await _context.EmployeeLessons.AnyAsync(el => el.Lesson.CourseId == id))
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "لا يمكن حذف الكورس لأنه يحتوي على تقدم لموظفين داخل الدروس."
                };
            }

            var deleted = await _courseRepo.Delete(course);
            return deleted
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "تم حذف الكورس بنجاح." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "فشل حذف الكورس." };
        }

        public async Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(c => c.Id == id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "الكورس غير موجود." };
            }

            course.IsPublished = !course.IsPublished;
            var updated = await _courseRepo.UpdateAsync(course);

            return updated
                ? new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Data = course.IsPublished,
                    Message = course.IsPublished ? "تم نشر الكورس." : "تم إلغاء نشر الكورس."
                }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "حدث خطأ أثناء تحديث حالة النشر." };
        }

        private async Task<ServiceResult<bool>> ValidateCourseAsync(CourseDto dto, int companyId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return new ServiceResult<bool> { IsSuccess = false, Message = "عنوان الكورس مطلوب." };

            if (string.IsNullOrWhiteSpace(dto.Description))
                return new ServiceResult<bool> { IsSuccess = false, Message = "وصف الكورس مطلوب." };

            if (dto.DurationInHours <= 0)
                return new ServiceResult<bool> { IsSuccess = false, Message = "عدد ساعات الكورس يجب أن يكون أكبر من صفر." };

            if (dto.EndDate < dto.StartDate)
                return new ServiceResult<bool> { IsSuccess = false, Message = "تاريخ النهاية يجب أن يكون بعد أو يساوي تاريخ البداية." };

            var category = await _categoryRepo.GetOneAsync(c => c.Id == dto.CategoryId && c.CompanyId == companyId);
            if (category == null)
                return new ServiceResult<bool> { IsSuccess = false, Message = "القسم المحدد غير موجود أو لا يتبع شركتك." };

            if (dto.InstructorId.HasValue)
            {
                var instructor = await _instructorRepo.GetOneAsync(i => i.Id == dto.InstructorId.Value && i.CompanyId == companyId);
                if (instructor == null)
                    return new ServiceResult<bool> { IsSuccess = false, Message = "المدرب المحدد غير موجود أو لا يتبع شركتك." };
            }

            return new ServiceResult<bool> { IsSuccess = true, Data = true };
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
