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
    }
}
