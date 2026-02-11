using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepo _courseRepo;

        public CourseServices(ICourseRepo courseRepo)
        {
            _courseRepo = courseRepo;
        }

        public async Task<List<CourseVm>> GetCourse(int categoryId)
        {
            var courses = await _courseRepo.GetCourseByCategoryIdAsync(categoryId);

            return courses.Select(c => new CourseVm
            {
                Id = c.Id,
                CourseName = c.Title,
                Description = c.Description,
                InstructorName = c.Instructor != null
                    ? c.Instructor.FullName
                    : "Not Assigned",
                DurationInHours = c.DurationInHours,
                logoUrl = string.IsNullOrEmpty(c.logo)
            ? "/images/default-course.png"
            : "/uploads/courses/" + c.logo

            }).ToList();
        }
        public async Task AddCourse(int categoryId, CreateCourseVm model)
        {
            var courses = await _courseRepo.GetCourseByCategoryIdAsync(categoryId);

            var course = new Courses
            {
                Title = model.CourseName,
                Description = model.Description,
                DurationInHours = model.DurationInHours,
                InstructorId = model.InstructorId,
                logo = model.LogoPath,
                CategoryId = categoryId
            };
            await _courseRepo.CreateAsync(course);
        }
        public async Task<CourseDetailsVm?> GetCourseDetails(int courseId)
        {
            // هنا بنادي الريبو يجيب الكورس بالدروس بتاعته
            var course = await _courseRepo.GetCourseWithLessonsAsync(courseId);

            if (course == null) return null;

            return new CourseDetailsVm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DurationInHours = course.DurationInHours,

                InstructorName = course.Instructor?.FullName ?? "N/A",
                
                Lessons = course.Lessons.Select(l => new LessonDisplayVm
                {
                    Id = l.Id,
                    Title = l.Title,
                    ContentUrl=l.VideoUrl,
                    Order = l.Order,
                    Description=l.Content
                    // أضف أي حقول أخرى للدروس هنا
                }).ToList()
            };
        }
    }
}
