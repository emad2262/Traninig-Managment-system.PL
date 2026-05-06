using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeLessonService : EmployeeWorkspaceServiceBase, IEmployeeLessonService
    {
        public EmployeeLessonService(
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

        public async Task<EmployeeLessonWatchVm?> GetLessonAsync(string userId, int lessonId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var lesson = await LessonRepo.GetOneAsync(l => l.Id == lessonId);
            if (lesson == null) return null;

            var context = await LoadCourseContextAsync(employee.Id, lesson.CourseId);
            if (context == null) return null;

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var lessonState = snapshot.AllLessons.FirstOrDefault(l => l.Lesson.Id == lessonId);
            if (lessonState == null) return null;

            await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);

            var orderedLessons = snapshot.AllLessons.OrderBy(l => l.SortKey).ToList();
            var currentIndex = orderedLessons.FindIndex(l => l.Lesson.Id == lessonId);
            var previousLessonId = currentIndex > 0 ? (int?)orderedLessons[currentIndex - 1].Lesson.Id : null;
            var nextLessonId = orderedLessons.Skip(currentIndex + 1).FirstOrDefault(l => l.IsUnlocked)?.Lesson.Id;

            return new EmployeeLessonWatchVm
            {
                LessonId = lessonState.Lesson.Id,
                CourseId = context.Course.Id,
                CourseTitle = context.Course.Title,
                Title = lessonState.Lesson.Title,
                Description = lessonState.Lesson.Content,
                ChapterTitle = context.Course.Chapters
                    .FirstOrDefault(ch => ch.Id == lessonState.Lesson.ChapterId)?.Title,
                VideoUrl = lessonState.Lesson.VideoUrl,
                PdfUrl = lessonState.Lesson.PdfUrl,
                IsCompleted = lessonState.IsCompleted,
                IsUnlocked = lessonState.IsUnlocked,
                CompletedLessons = snapshot.CompletedLessons,
                TotalLessons = snapshot.TotalLessons,
                CourseProgressPercentage = snapshot.ProgressPercentage,
                PreviousLessonId = previousLessonId,
                NextLessonId = nextLessonId
            };
        }

        public async Task<ServiceResult<EmployeeLessonCompletionResultVm>> MarkLessonCompletedAsync(string userId, int lessonId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null)
            {
                return Fail<EmployeeLessonCompletionResultVm>("Employee account not found.");
            }

            var lesson = await LessonRepo.GetOneAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return Fail<EmployeeLessonCompletionResultVm>("Lesson not found.");
            }

            var context = await LoadCourseContextAsync(employee.Id, lesson.CourseId);
            if (context == null)
            {
                return Fail<EmployeeLessonCompletionResultVm>("This course is not assigned to the current employee.");
            }

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var lessonState = snapshot.AllLessons.FirstOrDefault(l => l.Lesson.Id == lessonId);
            if (lessonState == null)
            {
                return Fail<EmployeeLessonCompletionResultVm>("Lesson not found in this course.");
            }

            if (!lessonState.IsUnlocked)
            {
                return Fail<EmployeeLessonCompletionResultVm>("Finish the required lessons before this one.");
            }

            double extraPoints = 0;
            var progress = await EmployeeLessonRepo.GetOneAsync(
                el => el.EmployeeId == employee.Id && el.LessonId == lessonId);

            if (progress == null)
            {
                progress = new EmployeeLesson
                {
                    EmployeeId = employee.Id,
                    LessonId = lessonId,
                    IsCompleted = true,
                    WatchedPercentage = 100,
                    LastWatchedSecond = 0,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                };

                await EmployeeLessonRepo.CreateAsync(progress);
                context.LessonRecords.Add(progress);
                extraPoints += LessonCompletionPoints;
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.WatchedPercentage = 100;
                progress.CompletedAt = DateTime.UtcNow;
                await EmployeeLessonRepo.UpdateAsync(progress);

                var local = context.LessonRecords.FirstOrDefault(l => l.LessonId == lessonId);
                if (local != null)
                {
                    local.IsCompleted = true;
                    local.WatchedPercentage = 100;
                    local.CompletedAt = progress.CompletedAt;
                }

                extraPoints += LessonCompletionPoints;
            }

            var updatedSnapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var sync = await PersistCourseStateAsync(employee, context.Assignment, context.Course, updatedSnapshot, extraPoints, updateLastAccess: true);

            return Ok(new EmployeeLessonCompletionResultVm
            {
                CourseId = context.Course.Id,
                LessonId = lessonId,
                NextLessonId = updatedSnapshot.NextLessonId,
                CourseCompleted = sync.CourseCompleted,
                CertificateAvailable = sync.CertificateIssued,
                CertificatePending = sync.CertificatePending,
                CertificateStatusText = sync.CertificateStatusText
            }, sync.CertificateIssued
                ? "Course completed successfully. Your certificate is ready."
                : sync.CertificatePending
                    ? "Course completed. Your company has been notified to issue the certificate."
                : "Lesson marked as completed.");
        }
    }
}
