using Microsoft.EntityFrameworkCore;
namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Course>, ICourseRepo
    {
        private const double LessonCompletionPoints = 10;
        private readonly ApplicationDbContext _context;

        public CoursesRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> DeleteCourseWithRelatedDataAsync(int courseId)
        {
            var course = await _context.courses
                .Include(c => c.Lessons)
                .Include(c => c.Exams)
                .Include(c => c.Chapters)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return false;
            }

            var lessonIds = course.Lessons.Select(l => l.Id).ToList();
            var examIds = course.Exams.Select(e => e.Id).ToList();

            var employeeCourses = await _context.EmployeeCourses
                .Where(ec => ec.CourseId == courseId)
                .ToListAsync();

            var certificates = await _context.EmployeeCertificates
                .Where(c => c.CourseId == courseId)
                .ToListAsync();

            var employeeLessons = lessonIds.Count == 0
                ? new List<EmployeeLesson>()
                : await _context.EmployeeLessons
                    .Where(el => lessonIds.Contains(el.LessonId))
                    .ToListAsync();

            var employeeAttempts = examIds.Count == 0
                ? new List<EmployeeExamAttempt>()
                : await _context.EmployeeExamAttempts
                    .Where(ea => examIds.Contains(ea.ExamId))
                    .ToListAsync();

            var badgeReasons = examIds.Select(examId => $"chapter-exam:{examId}")
                .Append($"course-completion:{courseId}")
                .ToList();

            var employeeBadges = await _context.EmployeeBadges
                .Include(eb => eb.Badge)
                .Where(eb => eb.EarnedReason != null && badgeReasons.Contains(eb.EarnedReason))
                .ToListAsync();

            var affectedEmployeeIds = employeeCourses.Select(ec => ec.EmployeeId)
                .Concat(employeeLessons.Select(el => el.EmployeeId))
                .Concat(employeeBadges.Select(eb => eb.EmployeeId))
                .Distinct()
                .ToList();

            var affectedEmployees = affectedEmployeeIds.Count == 0
                ? new List<Employee>()
                : await _context.employees
                    .Where(e => affectedEmployeeIds.Contains(e.Id))
                    .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var employee in affectedEmployees)
                {
                    var lessonPoints = employeeLessons.Count(el => el.EmployeeId == employee.Id && el.IsCompleted) * LessonCompletionPoints;
                    var badgePoints = employeeBadges
                        .Where(eb => eb.EmployeeId == employee.Id)
                        .Sum(eb => eb.Badge?.Points ?? 0);

                    employee.Points = Math.Max(0, employee.Points - lessonPoints - badgePoints);
                }

                if (employeeBadges.Any())
                {
                    _context.EmployeeBadges.RemoveRange(employeeBadges);
                }

                if (employeeAttempts.Any())
                {
                    _context.EmployeeExamAttempts.RemoveRange(employeeAttempts);
                }

                if (employeeLessons.Any())
                {
                    _context.EmployeeLessons.RemoveRange(employeeLessons);
                }

                if (employeeCourses.Any())
                {
                    _context.EmployeeCourses.RemoveRange(employeeCourses);
                }

                if (certificates.Any())
                {
                    _context.EmployeeCertificates.RemoveRange(certificates);
                }

                if (course.Exams.Any())
                {
                    _context.Exams.RemoveRange(course.Exams);
                }

                if (course.Lessons.Any())
                {
                    _context.lessons.RemoveRange(course.Lessons);
                }

                if (course.Chapters.Any())
                {
                    _context.CourseChapters.RemoveRange(course.Chapters);
                }

                _context.courses.Remove(course);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Course>> GetRecentInstructorCoursesAsync(int companyId, int take)
        {
            return await _context.courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Where(c => c.Category.CompanyId == companyId && c.InstructorId != null)
                .OrderByDescending(c => c.Id)
                .Take(take)
                .ToListAsync();
        }
    }
}
