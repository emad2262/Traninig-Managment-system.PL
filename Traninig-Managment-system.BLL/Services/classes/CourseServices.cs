
using Traninig_Managment_system.BLL.Services.Interfaces;
using Traninig_Managment_system.DAL.Repo;


namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        
        private readonly ICourseRepo _courseRepo;

        public CourseServices(ICourseRepo courseRepo)
        {
            _courseRepo = courseRepo;
        }
       
        public async Task<IEnumerable<CourseDto>> GetAllInCategoryAsync(int companyId, int categoryId)
        {
            var courses = await _courseRepo.GetAllAsync(
                c => c.CategoryId == categoryId && c.Category.CompanyId == companyId,
                c => c.Instructor
            );
            return courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                DurationInHours = c.DurationInHours,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Logo = c.logo,
                CategoryId = c.CategoryId,
                InstructorId = c.InstructorId,
                IsPublished = c.IsPublished
            });
        }

        public async Task<CourseDto> GetByIdAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                filter: c => c.Id == id && c.Category.CompanyId == companyId,
                includes: c => c.Instructor
            );

            if (course == null) return null!;

            return new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DurationInHours = course.DurationInHours,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Logo = course.logo,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId,
                IsPublished = course.IsPublished
            };
        }
        public async Task<ServiceResult<int>> CreateCourseAsync(CourseDto dto, int companyId)
        {
            if (dto.EndDate < dto.StartDate)
                return new ServiceResult<int>
                {
                    IsSuccess = false,
                    Message = "حدث خطأ أثناء حفظ الكورس."
                };


            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                DurationInHours = dto.DurationInHours,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                logo = dto.Logo ?? string.Empty,
                CategoryId = dto.CategoryId,
                InstructorId = dto.InstructorId,
                IsPublished = false
            };

            var isSaved = await _courseRepo.CreateAsync(course);

            if (!isSaved)
            {
                return new ServiceResult<int>
                {
                    IsSuccess = false,
                    Message = "حدث خطأ أثناء حفظ الكورس."
                };
            }

            return new ServiceResult<int>
            {
                IsSuccess = true,
                Data = course.Id
            };
        }

        public async Task<ServiceResult<bool>> UpdateAsync(CourseDto dto, int companyId)
        {
            if (dto.EndDate <= dto.StartDate)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "تاريخ النهاية يجب أن يكون بعد تاريخ البداية."
                };
            }

            var course = await _courseRepo.GetOneAsync(c => c.Id == dto.Id && c.Category.CompanyId == companyId);

            if (course == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "الكورس غير موجود أو لا تملك صلاحية تعديله."
                };
            }

            // تحديث البيانات
            course.Title = dto.Title;
            course.Description = dto.Description;
            course.DurationInHours = dto.DurationInHours;
            course.StartDate = dto.StartDate;
            course.EndDate = dto.EndDate;
            course.CategoryId = dto.CategoryId;
            course.InstructorId = dto.InstructorId;

            // لو اليوزر رفع صورة جديدة وبعت مسارها، بنحدثها، غير كده بنسيب القديمة
            if (!string.IsNullOrEmpty(dto.Logo))
            {
                course.logo = dto.Logo;
            }

            var isUpdated = await _courseRepo.UpdateAsync(course);

            if (!isUpdated)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "حدث خطأ أثناء تعديل الكورس."
                };
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "تم تعديل الكورس بنجاح."
            };
        }
        public async Task<ServiceResult<bool>> DeleteAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == id && c.Category.CompanyId == companyId
            );

            if (course == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "الكورس غير موجود"
                };
            }

            var result = await _courseRepo.Delete(course);

            if (!result)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "فشل الحذف"
                };
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true
            };
        }

        public async Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == id && c.Category.CompanyId == companyId
            );

            if (course == null)
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "الكورس غير موجود."
                };

            course.IsPublished = !course.IsPublished;

            var isUpdated = await _courseRepo.UpdateAsync(course);

            return isUpdated
                ? new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Data = course.IsPublished,
                    Message = course.IsPublished ? "تم نشر الكورس." : "تم إلغاء النشر."
                }
                : new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "حدث خطأ أثناء تحديث الحالة."
                };
        }
    }
}
