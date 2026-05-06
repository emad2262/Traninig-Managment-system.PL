namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorDashboardService : InstructorServiceBase, IInstructorDashboardService
    {
        private readonly ILessonRepo _lessonRepo;
        private readonly IExamRepo _examRepo;
        private readonly IInstructorProgressService _progressService;

        public InstructorDashboardService(
            IInstructorRepo instructorRepo,
            ICourseRepo courseRepo,
            ICourseChapterRepo courseChapterRepo,
            ILessonRepo lessonRepo,
            IExamRepo examRepo,
            IInstructorProgressService progressService)
            : base(instructorRepo, courseRepo, courseChapterRepo)
        {
            _lessonRepo = lessonRepo;
            _examRepo = examRepo;
            _progressService = progressService;
        }

        public async Task<InstructorDashboardVm?> GetDashboardAsync(string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var courses = (await CourseRepo.GetAllAsync(
                    c => c.InstructorId == instructor.Id,
                    c => c.Category,
                    c => c.Chapters,
                    c => c.Lessons,
                    c => c.EmployeeCourses,
                    c => c.Exams))
                .OrderBy(c => c.Category.Name)
                .ThenBy(c => c.Title)
                .ToList();

            var courseCards = courses.Select(MapCourseCard).ToList();
            var employeeRows = (await _progressService.GetEmployeeProgressAsync(userId)).Take(8).ToList();

            return new InstructorDashboardVm
            {
                InstructorId = instructor.Id,
                InstructorName = instructor.FullName,
                TotalCourses = courses.Count,
                PublishedCourses = courses.Count(c => c.IsPublished),
                TotalChapters = courses.Sum(c => c.Chapters.Count),
                TotalLessons = courses.Sum(c => c.Lessons.Count),
                TotalExams = courses.Sum(c => c.Exams.Count),
                TotalAssignedEmployees = courses
                    .SelectMany(c => c.EmployeeCourses)
                    .Select(ec => ec.EmployeeId)
                    .Distinct()
                    .Count(),
                AverageProgress = courses.SelectMany(c => c.EmployeeCourses).Any()
                    ? Math.Round(courses.SelectMany(c => c.EmployeeCourses).Average(ec => ec.Progress), 1)
                    : 0,
                Courses = courseCards,
                Categories = courseCards
                    .GroupBy(c => c.CategoryName)
                    .Select(g => new InstructorCategoryVm
                    {
                        CategoryName = g.Key,
                        Courses = g.ToList()
                    })
                    .ToList(),
                RecentProgress = employeeRows
            };
        }

        public async Task<InstructorCourseDetailsVm?> GetCourseDetailsAsync(int courseId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var course = await CourseRepo.GetOneAsync(
                c => c.Id == courseId && c.InstructorId == instructor.Id,
                c => c.Category,
                c => c.EmployeeCourses,
                c => c.Lessons);

            if (course == null)
            {
                return null;
            }

            var chapters = (await CourseChapterRepo.GetAllAsync(ch => ch.CourseId == courseId))
                .OrderBy(ch => ch.Order)
                .ThenBy(ch => ch.Title)
                .ToList();

            var lessons = (await _lessonRepo.GetAllAsync(
                    l => l.CourseId == courseId,
                    l => l.EmployeeLessons,
                    l => l.Chapter!))
                .ToList();

            var exams = (await _examRepo.GetAllAsync(
                    e => e.CourseId == courseId,
                    e => e.Questions,
                    e => e.Chapter!))
                .ToList();

            var employees = await _progressService.GetEmployeeProgressAsync(userId, courseId);

            return new InstructorCourseDetailsVm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = string.IsNullOrWhiteSpace(course.logo) ? null : course.logo,
                CategoryName = course.Category.Name,
                IsPublished = course.IsPublished,
                DurationInHours = course.DurationInHours,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                ChapterCount = chapters.Count,
                LessonCount = lessons.Count,
                ExamCount = exams.Count,
                AssignedEmployees = course.EmployeeCourses.Count,
                AverageProgress = course.EmployeeCourses.Any()
                    ? Math.Round(course.EmployeeCourses.Average(ec => ec.Progress), 1)
                    : 0,
                Chapters = chapters
                    .Select(ch => MapChapter(
                        ch,
                        lessons.Where(l => l.ChapterId == ch.Id),
                        exams.Where(e => e.ChapterId == ch.Id)))
                    .ToList(),
                Lessons = lessons
                    .Where(l => !l.ChapterId.HasValue)
                    .OrderBy(l => l.Order)
                    .Select(l => MapLesson(l))
                    .ToList(),
                Employees = employees.ToList(),
                Exams = exams
                    .Where(e => !e.ChapterId.HasValue)
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => MapExam(e))
                    .ToList()
            };
        }
    }
}
