using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeExamService : EmployeeWorkspaceServiceBase, IEmployeeExamService
    {
        public EmployeeExamService(
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

        public async Task<EmployeeExamTakeVm?> GetExamAsync(string userId, int examId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var exam = await ExamRepo.GetOneAsync(
                e => e.Id == examId && e.IsPublished,
                e => e.Course,
                e => e.Chapter!,
                e => e.Questions);

            if (exam == null) return null;

            var context = await LoadCourseContextAsync(employee.Id, exam.CourseId);
            if (context == null) return null;

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var examState = snapshot.AllExams.FirstOrDefault(e => e.Exam.Id == examId);
            if (examState == null || !examState.IsUnlocked)
            {
                return null;
            }

            await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);

            return new EmployeeExamTakeVm
            {
                ExamId = exam.Id,
                CourseId = exam.CourseId,
                CourseTitle = context.Course.Title,
                ChapterId = exam.ChapterId,
                ChapterTitle = exam.Chapter?.Title,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                PassingScore = exam.PassingScore,
                Questions = exam.Questions
                    .OrderBy(q => q.Id)
                    .Select(q => new EmployeeExamQuestionVm
                    {
                        QuestionId = q.Id,
                        Text = q.Text,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD
                    })
                    .ToList()
            };
        }

        public async Task<ServiceResult<EmployeeExamResultVm>> SubmitExamAsync(string userId, EmployeeExamSubmissionVm model)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null)
            {
                return Fail<EmployeeExamResultVm>("Employee account not found.");
            }

            var exam = await ExamRepo.GetOneAsync(
                e => e.Id == model.ExamId && e.IsPublished,
                e => e.Course,
                e => e.Chapter!,
                e => e.Questions);

            if (exam == null)
            {
                return Fail<EmployeeExamResultVm>("Exam not found.");
            }

            var context = await LoadCourseContextAsync(employee.Id, exam.CourseId);
            if (context == null)
            {
                return Fail<EmployeeExamResultVm>("This course is not assigned to the current employee.");
            }

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var examState = snapshot.AllExams.FirstOrDefault(e => e.Exam.Id == exam.Id);
            if (examState == null || !examState.IsUnlocked)
            {
                return Fail<EmployeeExamResultVm>("Finish the chapter lessons before taking this exam.");
            }

            var answers = (model.Answers ?? new List<EmployeeExamAnswerVm>())
                .ToDictionary(a => a.QuestionId, a => NormalizeOption(a.SelectedOption));

            var totalPoints = exam.Questions.Sum(q => Math.Max(q.Points, 1));
            var earnedPoints = 0;
            var correctAnswers = 0;

            foreach (var question in exam.Questions)
            {
                if (answers.TryGetValue(question.Id, out var selected) &&
                    selected.Equals(NormalizeOption(question.CorrectOption), StringComparison.Ordinal))
                {
                    earnedPoints += Math.Max(question.Points, 1);
                    correctAnswers++;
                }
            }

            var scorePercentage = totalPoints == 0
                ? 0
                : Math.Round(earnedPoints * 100d / totalPoints, 1);

            var attempt = new EmployeeExamAttempt
            {
                EmployeeId = employee.Id,
                ExamId = exam.Id,
                CorrectAnswers = correctAnswers,
                TotalQuestions = exam.Questions.Count,
                ScorePercentage = scorePercentage,
                IsPassed = scorePercentage >= exam.PassingScore,
                SubmittedAt = DateTime.UtcNow
            };

            await EmployeeExamAttemptRepo.CreateAsync(attempt);
            context.Attempts.Add(attempt);

            EmployeeBadgeAwardResult? examBadge = null;
            if (attempt.IsPassed)
            {
                examBadge = await AwardChapterExamBadgeAsync(employee.Id, exam, scorePercentage);
            }

            var updatedSnapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var sync = await PersistCourseStateAsync(
                employee,
                context.Assignment,
                context.Course,
                updatedSnapshot,
                examBadge?.PointsDelta ?? 0,
                updateLastAccess: true);

            var attemptCount = context.Attempts.Count(a => a.ExamId == exam.Id);

            return Ok(new EmployeeExamResultVm
            {
                ExamId = exam.Id,
                CourseId = exam.CourseId,
                CourseTitle = context.Course.Title,
                ExamTitle = exam.Title,
                ChapterTitle = exam.Chapter?.Title,
                ScorePercentage = scorePercentage,
                PassingScore = exam.PassingScore,
                IsPassed = attempt.IsPassed,
                CorrectAnswers = correctAnswers,
                TotalQuestions = exam.Questions.Count,
                AttemptCount = attemptCount,
                CertificateAvailable = sync.CertificateIssued,
                CertificatePending = sync.CertificatePending,
                CertificateStatusText = sync.CertificateStatusText,
                CourseCompleted = sync.CourseCompleted,
                AwardedBadgeName = examBadge?.BadgeName,
                AwardedBadgeTier = examBadge?.Tier
            }, attempt.IsPassed
                ? "Excellent. You passed the exam."
                : "Exam submitted. Review the chapter and try again.");
        }
    }
}
