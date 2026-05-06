using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeDashboardService : EmployeeWorkspaceServiceBase, IEmployeeDashboardService
    {
        public EmployeeDashboardService(
            IEmployeeRepo employeeRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            ICourseRepo courseRepo,
            ILessonRepo lessonRepo,
            IEmployeeLessonRepo employeeLessonRepo,
            IExamRepo examRepo,
            IEmployeeExamAttemptRepo employeeExamAttemptRepo,
            IEmployeeBadgeRepo employeeBadgeRepo,
            IBadgeRepo badgeRepo,
            ICompanyRepo companyRepo,
            IEmployeeCertificateRepo certificateRepo,
            ICompanyNotificationRepo companyNotificationRepo)
            : base(
                employeeRepo,
                employeeCourseRepo,
                courseRepo,
                lessonRepo,
                employeeLessonRepo,
                examRepo,
                employeeExamAttemptRepo,
                employeeBadgeRepo,
                badgeRepo,
                companyRepo,
                certificateRepo,
                companyNotificationRepo)
        {
        }

        public async Task<EmployeeDashboardVm?> GetDashboardAsync(string userId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var assignments = (await EmployeeCourseRepo.GetAllAsync(ec => ec.EmployeeId == employee.Id)).ToList();
            if (!assignments.Any())
            {
                var emptyBadges = await GetEmployeeBadgesAsync(employee.Id);
                return new EmployeeDashboardVm
                {
                    EmployeeName = employee.Name,
                    TotalPoints = employee.Points,
                    Badges = emptyBadges
                };
            }

            var courseIds = assignments.Select(ec => ec.CourseId).Distinct().ToList();
            var courses = (await CourseRepo.GetAllAsync(
                c => courseIds.Contains(c.Id),
                c => c.Category,
                c => c.Instructor!,
                c => c.Chapters,
                c => c.Lessons,
                c => c.Exams)).ToDictionary(c => c.Id);

            var badges = await GetEmployeeBadgesAsync(employee.Id);
            var cards = new List<EmployeeCourseCardVm>();
            var totalCompletedLessons = 0;
            var totalProgress = 0d;
            var completedCourses = 0;
            var certificateCount = 0;

            foreach (var assignment in assignments)
            {
                if (!courses.TryGetValue(assignment.CourseId, out var course))
                {
                    continue;
                }

                var lessonRecords = (await EmployeeLessonRepo.GetAllAsync(
                    el => el.EmployeeId == employee.Id && el.Lesson.CourseId == course.Id)).ToList();

                var attempts = (await EmployeeExamAttemptRepo.GetAllAsync(
                    ea => ea.EmployeeId == employee.Id && ea.Exam.CourseId == course.Id,
                    ea => ea.Exam)).ToList();

                var snapshot = BuildCourseSnapshot(course, lessonRecords, attempts);
                var sync = await PersistCourseStateAsync(employee, assignment, course, snapshot, 0, updateLastAccess: false);

                totalCompletedLessons += snapshot.CompletedLessons;
                totalProgress += snapshot.ProgressPercentage;
                if (snapshot.IsCompleted) completedCourses++;
                if (sync.CertificateIssued) certificateCount++;

                cards.Add(new EmployeeCourseCardVm
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    CategoryName = course.Category?.Name ?? "Training",
                    InstructorName = course.Instructor?.FullName ?? "Instructor",
                    Status = MapStatus(snapshot.IsCompleted, snapshot.ProgressPercentage),
                    ProgressPercentage = snapshot.ProgressPercentage,
                    CompletedLessons = snapshot.CompletedLessons,
                    TotalLessons = snapshot.TotalLessons,
                    CertificateAvailable = sync.CertificateIssued,
                    CertificatePending = sync.CertificatePending,
                    CertificateStatusText = sync.CertificateStatusText,
                    NextLessonId = snapshot.NextLessonId,
                    Highlight = sync.CertificateIssued
                        ? "Certificate ready"
                        : sync.CertificatePending
                            ? "Waiting for company approval"
                        : snapshot.NextLessonId.HasValue
                            ? "Continue from next lesson"
                            : snapshot.HasUnlockedExam
                                ? "Chapter exam unlocked"
                                : "Waiting for next milestone"
                });
            }

            var attemptsForActivity = (await EmployeeExamAttemptRepo.GetAllAsync(
                ea => ea.EmployeeId == employee.Id,
                ea => ea.Exam)).ToList();

            return new EmployeeDashboardVm
            {
                EmployeeName = employee.Name,
                TotalCourses = cards.Count,
                CompletedCourses = completedCourses,
                AverageProgress = cards.Any() ? Math.Round(totalProgress / cards.Count, 1) : 0,
                TotalPoints = employee.Points,
                TotalCompletedLessons = totalCompletedLessons,
                CertificatesCount = certificateCount,
                Courses = cards.OrderByDescending(c => c.CertificateAvailable)
                    .ThenByDescending(c => c.ProgressPercentage)
                    .ThenBy(c => c.Title)
                    .ToList(),
                Badges = badges,
                RecentActivity = BuildActivities(badges, attemptsForActivity)
            };
        }

        public async Task<EmployeeCourseDetailsVm?> GetCourseDetailsAsync(string userId, int courseId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var context = await LoadCourseContextAsync(employee.Id, courseId);
            if (context == null) return null;

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var sync = await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);

            var badges = await GetEmployeeBadgesAsync(employee.Id);

            return new EmployeeCourseDetailsVm
            {
                CourseId = context.Course.Id,
                Title = context.Course.Title,
                Description = context.Course.Description,
                CategoryName = context.Course.Category?.Name ?? "Training",
                InstructorName = context.Course.Instructor?.FullName ?? "Instructor",
                Status = MapStatus(snapshot.IsCompleted, snapshot.ProgressPercentage),
                ProgressPercentage = snapshot.ProgressPercentage,
                CompletedLessons = snapshot.CompletedLessons,
                TotalLessons = snapshot.TotalLessons,
                FinalScore = snapshot.FinalScore,
                CertificateAvailable = sync.CertificateIssued,
                CertificatePending = sync.CertificatePending,
                CertificateStatusText = sync.CertificateStatusText,
                NextLessonId = snapshot.NextLessonId,
                CompletedAt = context.Assignment.CompletedAt,
                GeneralLessons = snapshot.GeneralLessons.Select(l => MapLessonRow(l)).ToList(),
                FinalAssessments = snapshot.FinalAssessments.Select(MapExamSummary).ToList(),
                Chapters = snapshot.Chapters.Select(ch => new EmployeeChapterProgressVm
                {
                    ChapterId = ch.Chapter.Id,
                    Order = ch.Chapter.Order,
                    Title = ch.Chapter.Title,
                    Description = ch.Chapter.Description,
                    IsUnlocked = ch.IsUnlocked,
                    IsCompleted = ch.IsCompleted,
                    CompletedLessons = ch.Lessons.Count(l => l.IsCompleted),
                    TotalLessons = ch.Lessons.Count,
                    Lessons = ch.Lessons.Select(l => MapLessonRow(l, ch.Chapter.Title)).ToList(),
                    Exam = ch.Exam == null ? null : MapExamSummary(ch.Exam)
                }).ToList(),
                EarnedBadges = badges.Take(8).ToList()
            };
        }

        public async Task<EmployeeCertificateVm?> GetCertificateAsync(string userId, int courseId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var context = await LoadCourseContextAsync(employee.Id, courseId);
            if (context == null) return null;

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var sync = await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);
            if (!sync.CertificateIssued)
            {
                return null;
            }

            var certificate = await CertificateRepo.GetIssuedForEmployeeCourseAsync(employee.Id, courseId);
            if (certificate == null)
            {
                return null;
            }

            var completedAt = certificate.CompletedAt;
            var company = await CompanyRepo.GetOneAsync(c => c.Id == employee.CompanyId);
            var companyName = company?.Name ?? "Training Company";
            var instructorName = context.Course.Instructor?.FullName ?? "Instructor";

            return new EmployeeCertificateVm
            {
                CourseId = context.Course.Id,
                CourseTitle = context.Course.Title,
                EmployeeName = employee.Name,
                CompanyName = companyName,
                InstructorName = instructorName,
                DurationInHours = context.Course.DurationInHours,
                FinalScore = snapshot.FinalScore,
                CompletedAt = completedAt,
                IssuedAt = certificate.IssuedAt,
                CertificateNumber = certificate.CertificateNumber
            };
        }
    }
}
