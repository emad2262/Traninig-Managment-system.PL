using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepo _courseRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IInstructorRepo _instructorRepo;
        private readonly ICompanySubscriptionService _subscriptionService;

        public CourseServices(
            ICourseRepo courseRepo,
            ICategoryRepo categoryRepo,
            IInstructorRepo instructorRepo,
            ICompanySubscriptionService subscriptionService)
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _instructorRepo = instructorRepo;
            _subscriptionService = subscriptionService;
        }

        public async Task<IEnumerable<CourseDto>> GetAllInCategoryAsync(int companyId, int categoryId)
        {
            var courses = await _courseRepo.GetAllAsync(
                c => c.CategoryId == categoryId && c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            return courses.Select(MapCourseDto)
                .OrderBy(c => c.StartDate)
                .ThenBy(c => c.Title)
                .ToList();
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesAsync(int companyId)
        {
            var courses = await _courseRepo.GetAllAsync(
                c => c.Category.CompanyId == companyId,
                c => c.Category,
                c => c.Instructor!);

            return courses.Select(MapCourseDto)
                .OrderBy(c => c.CategoryName)
                .ThenBy(c => c.Title)
                .ToList();
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
            var subscription = await _subscriptionService.EnsureCanCreateCourseAsync(companyId);
            if (!subscription.IsSuccess)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = subscription.Message };
            }

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
                ? new ServiceResult<int> { IsSuccess = true, Data = course.Id, Message = "Course created successfully." }
                : new ServiceResult<int> { IsSuccess = false, Message = "An error occurred while saving the course." };
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
                    Message = "The course was not found or does not belong to your company."
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
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "Course updated successfully." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "An error occurred while updating the course." };
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(c => c.Id == id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Course not found." };
            }

            var deleted = await _courseRepo.DeleteCourseWithRelatedDataAsync(id);
            if (!deleted)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "The course could not be deleted cleanly. Please try again."
                };
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "The course and all related assignments, progress, and assessments were deleted successfully."
            };
        }

        public async Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId)
        {
            var subscription = await _subscriptionService.EnsureActiveAsync(companyId);
            if (!subscription.IsSuccess)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = subscription.Message };
            }

            var course = await _courseRepo.GetOneAsync(c => c.Id == id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Course not found." };
            }

            course.IsPublished = !course.IsPublished;
            var updated = await _courseRepo.UpdateAsync(course);

            return updated
                ? new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Data = course.IsPublished,
                    Message = course.IsPublished ? "Course published successfully." : "Course moved back to draft."
                }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "An error occurred while updating publish state." };
        }

        public async Task<ServiceResult<bool>> UnassignInstructorAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(c => c.Id == id && c.Category.CompanyId == companyId);
            if (course == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Course not found." };
            }

            if (!course.InstructorId.HasValue)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "This course does not have an assigned instructor right now." };
            }

            course.InstructorId = null;
            var updated = await _courseRepo.UpdateAsync(course);

            return updated
                ? new ServiceResult<bool> { IsSuccess = true, Data = true, Message = "Instructor assignment removed successfully." }
                : new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Failed to remove the instructor assignment." };
        }

        private async Task<ServiceResult<bool>> ValidateCourseAsync(CourseDto dto, int companyId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return new ServiceResult<bool> { IsSuccess = false, Message = "Course title is required." };

            if (string.IsNullOrWhiteSpace(dto.Description))
                return new ServiceResult<bool> { IsSuccess = false, Message = "Course description is required." };

            if (dto.DurationInHours <= 0)
                return new ServiceResult<bool> { IsSuccess = false, Message = "Duration must be greater than zero." };

            if (dto.EndDate < dto.StartDate)
                return new ServiceResult<bool> { IsSuccess = false, Message = "End date must be after or equal to start date." };

            var category = await _categoryRepo.GetOneAsync(c => c.Id == dto.CategoryId && c.CompanyId == companyId);
            if (category == null)
                return new ServiceResult<bool> { IsSuccess = false, Message = "The selected category was not found for this company." };

            if (dto.InstructorId.HasValue)
            {
                var instructor = await _instructorRepo.GetOneAsync(i => i.Id == dto.InstructorId.Value && i.CompanyId == companyId);
                if (instructor == null)
                    return new ServiceResult<bool> { IsSuccess = false, Message = "The selected instructor was not found for this company." };
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
