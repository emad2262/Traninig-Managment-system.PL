using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Course;
using Traninig_Managment_system.BLL.Dtos.Lessons;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepo _courseRepo;

        public CourseServices(ICourseRepo courseRepo)
        {
            _courseRepo = courseRepo;
        }
       
        public  async Task<IEnumerable<ListCourseDto>> GetAllCoursesInCategoryAsync(int companyId, int categoryId)
        {
            var categorycourses =await _courseRepo.GetAllAsync(e=>e.CategoryId==categoryId && e.Category.CompanyId==companyId);
            return categorycourses.Select(e => new ListCourseDto
            {
                Id = e.Id,
                Title = e.Title,
                DurationInHours = e.DurationInHours,
                IsPublished = e.IsPublished,
                logo=e.logo,
                LessonCount = e.Lessons.Count

            }).ToList();
        }
        public async Task<IEnumerable<ListCourseDto>> GetCompanyCoursesAsync(int companyId)
        {
            var allcourses = await _courseRepo.GetAllAsync(e=>e.Category.CompanyId==companyId);
            return allcourses.Select(e => new ListCourseDto
            {
                Id = e.Id,
                Title = e.Title,
                DurationInHours = e.DurationInHours,
                IsPublished = e.IsPublished,
                logo = e.logo,
                LessonCount = e.Lessons.Count

            }).ToList();

        }
        public async Task<CourseDetailsDto> GetCourseDetailsAsync(int companyId, int id)
        {
            var course = await _courseRepo.GetOneAsync(
                e => e.Id == id && e.Category.CompanyId == companyId);

            return new CourseDetailsDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                logo = course.logo,
                DurationInHours = course.DurationInHours,
                IsPublished = course.IsPublished,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                InstructorName = course.Instructor?.FullName ?? "—",
                CategoryName = course.Category?.Name ?? "—",
                LessonsList = course.Lessons?
                    .OrderBy(e => e.Order)
                    .Select(e => new LessonListDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Content = e.Content,
                        VideoUrl = e.VideoUrl,
                        PdfUrl = e.PdfUrl,
                        Order = e.Order,
                        CreatedAt = e.CreatedAt
                    }).ToList() ?? new List<LessonListDto>()
            };
        }
        public async Task<UpdateCourseDto?> GetCourseForEditAsync(int companyId, int id)
        {
            var course = await _courseRepo.GetOneAsync(
                e => e.Id == id && e.Category.CompanyId == companyId);

            if (course is null)
                return null;

            return new UpdateCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = course.logo,
                DurationInHours = course.DurationInHours,
                IsPublish = course.IsPublished,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId
            };
        }
        public async Task<ServiceResult<int>> CreateCourseAsync(CreateCourseDto model, int companyId)
        {
            if (model.EndDate < model.StartDate)
            {
                return new ServiceResult<int>
                {
                    IsSuccess = false,
                    Message = "The end date cannot be earlier than the start date."
                };
            }

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                logo = model.logo,
                DurationInHours = model.DurationInHours,
                IsPublished = model.IsPublished,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CategoryId = model.CategoryId,
                InstructorId = model.InstructorId
            };

            await _courseRepo.CreateAsync(course);
            await _courseRepo.SaveChangesAsync();

            return new ServiceResult<int>
            {
                IsSuccess = true,
                Message = "Course created.",
                Data = course.Id
            };
        }
        public async Task<ServiceResult<bool>> EditCourseAsync(UpdateCourseDto model, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                e => e.Id == model.Id && e.Category.CompanyId == companyId);

            if (course is null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "Course not found."
                };
            }

            if (model.EndDate < model.StartDate)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "The end date cannot be earlier than the start date."
                };
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.logo = model.Logo;
            course.DurationInHours = model.DurationInHours;
            course.IsPublished = model.IsPublish;
            course.StartDate = model.StartDate;
            course.EndDate = model.EndDate;
            course.CategoryId = model.CategoryId;
            course.InstructorId = model.InstructorId;

            await _courseRepo.Update(course);
            await _courseRepo.SaveChangesAsync();

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Message = "Course updated."
            };
        }
        public async Task<ServiceResult<DeletedCourseFilesDto>> DeleteCourseAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                e => e.Id == id && e.Category.CompanyId == companyId);

            if (course is null)
            {
                return new ServiceResult<DeletedCourseFilesDto>
                {
                    IsSuccess = false,
                    Message = "Course not found."
                };
            }

            // ممنوع الحذف لو فيه موظفين متسجلين — الحل إنه يرجع Draft
            if (course.EmployeeCourses is not null && course.EmployeeCourses.Any())
            {
                return new ServiceResult<DeletedCourseFilesDto>
                {
                    IsSuccess = false,
                    Message = "This course has enrolled employees. Unpublish it instead of deleting."
                };
            }

            //// بجمّع مسارات الملفات قبل الحذف — بعد الحذف مش هيبقى فيه مكان أقراها منه
            //var files = new DeletedCourseFilesDto { Logo = course.logo };

            //if (course.Lessons is not null)
            //{
            //    foreach (var lesson in course.Lessons)
            //    {
            //        if (!string.IsNullOrWhiteSpace(lesson.VideoUrl))
            //            files.LessonFiles.Add(lesson.VideoUrl);

            //        if (!string.IsNullOrWhiteSpace(lesson.PdfUrl))
            //            files.LessonFiles.Add(lesson.PdfUrl);
            //    }
            //}

            await _courseRepo.Delete(course);
            await _courseRepo.SaveChangesAsync();

            return new ServiceResult<DeletedCourseFilesDto>
            {
                IsSuccess = true,
                Message = "Course deleted.",
            };
        }
        public async Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == id &&
                     c.Category.CompanyId == companyId);

            if (course == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "Course not found."
                };
            }

            course.IsPublished = !course.IsPublished;

            await _courseRepo.Update(course);

            await _courseRepo.SaveChangesAsync();

           
            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Message = course.IsPublished
                    ? "Course published successfully."
                    : "Course moved back to draft."
            };
        }
        public async Task<int> CourseCount(int companyId)
        {
            var coursecount = await _courseRepo.CountAsync(e => e.Category.CompanyId == companyId);
            return coursecount;
        }
        public async Task<int> PublishedCourseCount(int companyId)
        {
            var publishcoursse = await _courseRepo.CountAsync(e => e.Category.CompanyId ==
            companyId && e.IsPublished);

            return publishcoursse;
        }

    }
}
