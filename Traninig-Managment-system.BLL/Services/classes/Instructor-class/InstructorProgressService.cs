namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorProgressService : InstructorServiceBase, IInstructorProgressService
    {
        private readonly IEmployeeCourseRepo _employeeCourseRepo;
        private readonly IEmployeeLessonRepo _employeeLessonRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IEmployeeBadgeRepo _employeeBadgeRepo;

        public InstructorProgressService(
            IInstructorRepo instructorRepo,
            ICourseRepo courseRepo,
            ICourseChapterRepo courseChapterRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            IEmployeeLessonRepo employeeLessonRepo,
            IEmployeeRepo employeeRepo,
            IEmployeeBadgeRepo employeeBadgeRepo)
            : base(instructorRepo, courseRepo, courseChapterRepo)
        {
            _employeeCourseRepo = employeeCourseRepo;
            _employeeLessonRepo = employeeLessonRepo;
            _employeeRepo = employeeRepo;
            _employeeBadgeRepo = employeeBadgeRepo;
        }

        public async Task<IEnumerable<InstructorEmployeeProgressVm>> GetEmployeeProgressAsync(string userId, int? courseId = null)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Enumerable.Empty<InstructorEmployeeProgressVm>();
            }

            var assignments = (await _employeeCourseRepo.GetAllAsync(
                    ec => ec.Course.InstructorId == instructor.Id &&
                        (!courseId.HasValue || ec.CourseId == courseId.Value),
                    ec => ec.Employee,
                    ec => ec.Course))
                .ToList();

            if (!assignments.Any())
            {
                return Enumerable.Empty<InstructorEmployeeProgressVm>();
            }

            var courseIds = assignments.Select(ec => ec.CourseId).Distinct().ToList();
            var employeeIds = assignments.Select(ec => ec.EmployeeId).Distinct().ToList();

            var courses = (await CourseRepo.GetAllAsync(
                    c => courseIds.Contains(c.Id),
                    c => c.Lessons))
                .ToDictionary(c => c.Id);

            var completedLessonsLookup = (await _employeeLessonRepo.GetAllAsync(
                    el => employeeIds.Contains(el.EmployeeId) &&
                        el.IsCompleted &&
                        courseIds.Contains(el.Lesson.CourseId),
                    el => el.Lesson))
                .GroupBy(el => new { el.EmployeeId, el.Lesson.CourseId })
                .ToDictionary(g => (g.Key.EmployeeId, g.Key.CourseId), g => g.Count());

            var badgeCounts = (await _employeeBadgeRepo.GetAllAsync(eb => employeeIds.Contains(eb.EmployeeId)))
                .GroupBy(eb => eb.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            return assignments
                .Select(ec => new InstructorEmployeeProgressVm
                {
                    EmployeeId = ec.EmployeeId,
                    Name = ec.Employee.Name,
                    Email = ec.Employee.Email,
                    JobTitle = ec.Employee.JobTitle,
                    IsActive = ec.Employee.IsActive,
                    CourseId = ec.CourseId,
                    CourseName = ec.Course.Title,
                    Progress = Math.Round(ec.Progress, 1),
                    Status = ec.Status.ToString(),
                    FinalScore = ec.FinalScore,
                    AssignedAt = ec.AssignedAt,
                    LastAccessedAt = ec.LastAccessedAt,
                    CompletedAt = ec.CompletedAt,
                    TotalLessons = courses.TryGetValue(ec.CourseId, out var course) ? course.Lessons.Count : 0,
                    CompletedLessons = completedLessonsLookup.TryGetValue((ec.EmployeeId, ec.CourseId), out var completedLessons)
                        ? completedLessons
                        : 0,
                    BadgeCount = badgeCounts.TryGetValue(ec.EmployeeId, out var badgeCount) ? badgeCount : 0
                })
                .OrderByDescending(ec => ec.LastAccessedAt ?? ec.AssignedAt)
                .ToList();
        }

        public async Task<InstructorEmployeeDetailsVm?> GetEmployeeDetailsAsync(int employeeId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var employee = await _employeeRepo.GetOneAsync(e =>
                e.Id == employeeId &&
                e.CompanyId == instructor.CompanyId &&
                e.EmployeeCourses.Any(ec => ec.Course.InstructorId == instructor.Id));

            if (employee == null)
            {
                return null;
            }

            var progress = await GetEmployeeProgressAsync(userId);
            var badges = (await _employeeBadgeRepo.GetAllAsync(
                    eb => eb.EmployeeId == employee.Id,
                    eb => eb.Badge))
                .OrderByDescending(b => b.EarnedAt)
                .ToList();

            return new InstructorEmployeeDetailsVm
            {
                EmployeeId = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive,
                Points = employee.Points,
                Courses = progress.Where(p => p.EmployeeId == employee.Id).ToList(),
                Badges = badges
                    .Select(b => new InstructorBadgeVm
                    {
                        Name = b.Badge?.Name ?? string.Empty,
                        Tier = b.Badge?.Tier ?? string.Empty,
                        Points = b.Badge?.Points ?? 0,
                        EarnedAt = b.EarnedAt
                    })
                    .ToList()
            };
        }
    }
}
