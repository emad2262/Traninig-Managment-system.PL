using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeWorkspaceService : IEmployeeWorkspaceService
    {
        private const double LessonCompletionPoints = 10;

        private readonly IEmployeeRepo _employeeRepo;
        private readonly IEmployeeCourseRepo _employeeCourseRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly ILessonRepo _lessonRepo;
        private readonly IEmployeeLessonRepo _employeeLessonRepo;
        private readonly IExamRepo _examRepo;
        private readonly IEmployeeExamAttemptRepo _employeeExamAttemptRepo;
        private readonly IEmployeeBadgeRepo _employeeBadgeRepo;
        private readonly IBadgeRepo _badgeRepo;
        private readonly ICompanyRepo _companyRepo;

        public EmployeeWorkspaceService(
            IEmployeeRepo employeeRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            ICourseRepo courseRepo,
            ILessonRepo lessonRepo,
            IEmployeeLessonRepo employeeLessonRepo,
            IExamRepo examRepo,
            IEmployeeExamAttemptRepo employeeExamAttemptRepo,
            IEmployeeBadgeRepo employeeBadgeRepo,
            IBadgeRepo badgeRepo,
            ICompanyRepo companyRepo)
        {
            _employeeRepo = employeeRepo;
            _employeeCourseRepo = employeeCourseRepo;
            _courseRepo = courseRepo;
            _lessonRepo = lessonRepo;
            _employeeLessonRepo = employeeLessonRepo;
            _examRepo = examRepo;
            _employeeExamAttemptRepo = employeeExamAttemptRepo;
            _employeeBadgeRepo = employeeBadgeRepo;
            _badgeRepo = badgeRepo;
            _companyRepo = companyRepo;
        }

        public async Task<EmployeeDashboardVm?> GetDashboardAsync(string userId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var assignments = (await _employeeCourseRepo.GetAllAsync(ec => ec.EmployeeId == employee.Id)).ToList();
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
            var courses = (await _courseRepo.GetAllAsync(
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

                var lessonRecords = (await _employeeLessonRepo.GetAllAsync(
                    el => el.EmployeeId == employee.Id && el.Lesson.CourseId == course.Id)).ToList();

                var attempts = (await _employeeExamAttemptRepo.GetAllAsync(
                    ea => ea.EmployeeId == employee.Id && ea.Exam.CourseId == course.Id,
                    ea => ea.Exam)).ToList();

                var snapshot = BuildCourseSnapshot(course, lessonRecords, attempts);
                await PersistCourseStateAsync(employee, assignment, course, snapshot, 0, updateLastAccess: false);

                totalCompletedLessons += snapshot.CompletedLessons;
                totalProgress += snapshot.ProgressPercentage;
                if (snapshot.IsCompleted) completedCourses++;
                if (snapshot.IsCompleted) certificateCount++;

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
                    CertificateAvailable = snapshot.IsCompleted,
                    NextLessonId = snapshot.NextLessonId,
                    Highlight = snapshot.IsCompleted
                        ? "Certificate ready"
                        : snapshot.NextLessonId.HasValue
                            ? "Continue from next lesson"
                            : snapshot.HasUnlockedExam
                                ? "Chapter exam unlocked"
                                : "Waiting for next milestone"
                });
            }

            var attemptsForActivity = (await _employeeExamAttemptRepo.GetAllAsync(
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
            await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);

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
                CertificateAvailable = snapshot.IsCompleted,
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

        public async Task<EmployeeLessonWatchVm?> GetLessonAsync(string userId, int lessonId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var lesson = await _lessonRepo.GetOneAsync(l => l.Id == lessonId);
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

            var lesson = await _lessonRepo.GetOneAsync(l => l.Id == lessonId);
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
            var progress = await _employeeLessonRepo.GetOneAsync(
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

                await _employeeLessonRepo.CreateAsync(progress);
                context.LessonRecords.Add(progress);
                extraPoints += LessonCompletionPoints;
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.WatchedPercentage = 100;
                progress.CompletedAt = DateTime.UtcNow;
                await _employeeLessonRepo.UpdateAsync(progress);

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
                CertificateAvailable = sync.CertificateAvailable
            }, sync.CourseCompleted
                ? "Course completed successfully. Your certificate is ready."
                : "Lesson marked as completed.");
        }

        public async Task<EmployeeExamTakeVm?> GetExamAsync(string userId, int examId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var exam = await _examRepo.GetOneAsync(
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

            var exam = await _examRepo.GetOneAsync(
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

            await _employeeExamAttemptRepo.CreateAsync(attempt);
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
                CertificateAvailable = sync.CertificateAvailable,
                CourseCompleted = sync.CourseCompleted,
                AwardedBadgeName = examBadge?.BadgeName,
                AwardedBadgeTier = examBadge?.Tier
            }, attempt.IsPassed
                ? "Excellent. You passed the exam."
                : "Exam submitted. Review the chapter and try again.");
        }

        public async Task<EmployeeCertificateVm?> GetCertificateAsync(string userId, int courseId)
        {
            var employee = await ResolveEmployeeAsync(userId);
            if (employee == null) return null;

            var context = await LoadCourseContextAsync(employee.Id, courseId);
            if (context == null) return null;

            var snapshot = BuildCourseSnapshot(context.Course, context.LessonRecords, context.Attempts);
            var sync = await PersistCourseStateAsync(employee, context.Assignment, context.Course, snapshot, 0, updateLastAccess: true);
            if (!sync.CertificateAvailable)
            {
                return null;
            }

            var completedAt = context.Assignment.CompletedAt ?? DateTime.UtcNow;
            var company = await _companyRepo.GetOneAsync(c => c.Id == employee.CompanyId);
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
                CertificateNumber = BuildCertificateNumber(context.Course.Id, employee.Id, completedAt)
            };
        }

        private async Task<Employee?> ResolveEmployeeAsync(string userId)
        {
            return await _employeeRepo.GetOneAsync(e => e.UserId == userId && e.IsActive);
        }

        private async Task<EmployeeCourseContext?> LoadCourseContextAsync(int employeeId, int courseId)
        {
            var assignment = await _employeeCourseRepo.GetOneAsync(
                ec => ec.EmployeeId == employeeId && ec.CourseId == courseId);
            if (assignment == null) return null;

            var course = await _courseRepo.GetOneAsync(
                c => c.Id == courseId,
                c => c.Category,
                c => c.Instructor!,
                c => c.Chapters,
                c => c.Lessons,
                c => c.Exams);
            if (course == null) return null;

            var lessonRecords = (await _employeeLessonRepo.GetAllAsync(
                el => el.EmployeeId == employeeId && el.Lesson.CourseId == courseId)).ToList();

            var attempts = (await _employeeExamAttemptRepo.GetAllAsync(
                ea => ea.EmployeeId == employeeId && ea.Exam.CourseId == courseId,
                ea => ea.Exam)).ToList();

            return new EmployeeCourseContext
            {
                Assignment = assignment,
                Course = course,
                LessonRecords = lessonRecords,
                Attempts = attempts
            };
        }

        private async Task<List<EmployeeEarnedBadgeVm>> GetEmployeeBadgesAsync(int employeeId)
        {
            var employeeBadges = (await _employeeBadgeRepo.GetAllAsync(
                eb => eb.EmployeeId == employeeId,
                eb => eb.Badge))
                .OrderByDescending(eb => eb.EarnedAt)
                .ToList();

            return employeeBadges.Select(eb => new EmployeeEarnedBadgeVm
            {
                Name = eb.Badge?.Name ?? "Badge",
                Description = eb.Badge?.Description ?? string.Empty,
                BadgeType = eb.Badge?.BadgeType ?? "Achievement",
                Tier = eb.Badge?.Tier ?? "Bronze",
                Points = eb.Badge?.Points ?? 0,
                IconUrl = eb.Badge?.IconUrl,
                EarnedAt = eb.EarnedAt,
                Reason = HumanizeReason(eb.EarnedReason)
            }).ToList();
        }

        private List<EmployeeActivityItemVm> BuildActivities(
            IEnumerable<EmployeeEarnedBadgeVm> badges,
            IEnumerable<EmployeeExamAttempt> attempts)
        {
            var items = new List<EmployeeActivityItemVm>();

            items.AddRange(badges.Take(4).Select(b => new EmployeeActivityItemVm
            {
                Title = b.Name,
                Subtitle = $"{b.Tier} badge earned",
                HappenedAt = b.EarnedAt,
                Kind = "badge"
            }));

            items.AddRange(attempts
                .OrderByDescending(a => a.SubmittedAt)
                .Take(4)
                .Select(a => new EmployeeActivityItemVm
                {
                    Title = a.Exam?.Title ?? "Exam attempt",
                    Subtitle = a.IsPassed
                        ? $"Passed with {a.ScorePercentage:0.#}%"
                        : $"Needs review ({a.ScorePercentage:0.#}%)",
                    HappenedAt = a.SubmittedAt,
                    Kind = "exam"
                }));

            return items.OrderByDescending(i => i.HappenedAt).Take(6).ToList();
        }

        private EmployeeLessonRowVm MapLessonRow(LessonSnapshot lesson, string? chapterTitle = null)
        {
            return new EmployeeLessonRowVm
            {
                LessonId = lesson.Lesson.Id,
                Title = lesson.Lesson.Title,
                Description = lesson.Lesson.Content,
                Order = lesson.Lesson.Order,
                IsUnlocked = lesson.IsUnlocked,
                IsCompleted = lesson.IsCompleted,
                HasVideo = !string.IsNullOrWhiteSpace(lesson.Lesson.VideoUrl),
                HasPdf = !string.IsNullOrWhiteSpace(lesson.Lesson.PdfUrl),
                ChapterTitle = chapterTitle
            };
        }

        private EmployeeExamSummaryVm MapExamSummary(ExamSnapshot exam)
        {
            return new EmployeeExamSummaryVm
            {
                ExamId = exam.Exam.Id,
                Title = exam.Exam.Title,
                Description = exam.Exam.Description,
                PassingScore = exam.Exam.PassingScore,
                DurationMinutes = exam.Exam.DurationMinutes,
                IsUnlocked = exam.IsUnlocked,
                IsPassed = exam.IsPassed,
                BestScore = exam.BestScore,
                AttemptCount = exam.AttemptCount,
                StatusText = exam.IsPassed
                    ? $"Passed ({exam.BestScore:0.#}%)"
                    : exam.IsUnlocked
                        ? exam.AttemptCount > 0
                            ? $"Retry available - best {exam.BestScore:0.#}%"
                            : "Ready to take"
                        : "Locked until the chapter is finished",
                EarnedTier = exam.IsPassed ? ResolveTier(exam.BestScore ?? 0) : null
            };
        }

        private async Task<CourseSyncResult> PersistCourseStateAsync(
            Employee employee,
            EmployeeCourse assignment,
            Course course,
            CourseSnapshot snapshot,
            double extraPoints,
            bool updateLastAccess)
        {
            var now = DateTime.UtcNow;
            assignment.Progress = snapshot.ProgressPercentage;
            assignment.Status = snapshot.IsCompleted
                ? CourseStatus.Completed
                : snapshot.CompletedUnits > 0
                    ? CourseStatus.InProgress
                    : CourseStatus.NotStarted;
            assignment.FinalScore = snapshot.FinalScore;
            assignment.CompletedAt = snapshot.IsCompleted ? assignment.CompletedAt ?? now : null;
            if (updateLastAccess)
            {
                assignment.LastAccessedAt = now;
            }

            await _employeeCourseRepo.UpdateAsync(assignment);

            var totalPointsDelta = extraPoints;
            if (snapshot.IsCompleted)
            {
                var courseBadge = await AwardCourseCompletionBadgeAsync(employee.Id, course);
                totalPointsDelta += courseBadge?.PointsDelta ?? 0;
            }

            if (totalPointsDelta > 0)
            {
                employee.Points += totalPointsDelta;
                await _employeeRepo.UpdateAsync(employee);
            }

            return new CourseSyncResult
            {
                CourseCompleted = snapshot.IsCompleted,
                CertificateAvailable = snapshot.IsCompleted
            };
        }

        private async Task<EmployeeBadgeAwardResult?> AwardChapterExamBadgeAsync(int employeeId, Exam exam, double score)
        {
            var tier = ResolveTier(score);
            var points = tier switch
            {
                "Gold" => 40,
                "Silver" => 30,
                _ => 20
            };

            var badge = await EnsureBadgeAsync(
                name: $"Chapter Finisher - {tier}",
                description: "Awarded for passing a chapter exam.",
                badgeType: "ChapterExam",
                tier: tier,
                points: points);

            return await AwardOrUpgradeBadgeAsync(
                employeeId,
                badge,
                $"chapter-exam:{exam.Id}");
        }

        private async Task<EmployeeBadgeAwardResult?> AwardCourseCompletionBadgeAsync(int employeeId, Course course)
        {
            var badge = await EnsureBadgeAsync(
                name: "Course Completion - Platinum",
                description: "Awarded for completing every lesson and assessment in a course.",
                badgeType: "CourseCompletion",
                tier: "Platinum",
                points: 80);

            return await AwardOrUpgradeBadgeAsync(
                employeeId,
                badge,
                $"course-completion:{course.Id}");
        }

        private async Task<Badge> EnsureBadgeAsync(
            string name,
            string description,
            string badgeType,
            string tier,
            int points)
        {
            var badge = await _badgeRepo.GetOneAsync(b => b.Name == name);
            if (badge != null)
            {
                return badge;
            }

            var created = new Badge
            {
                Name = name,
                Description = description,
                BadgeType = badgeType,
                Tier = tier,
                Points = points
            };

            await _badgeRepo.CreateAsync(created);
            return created;
        }

        private async Task<EmployeeBadgeAwardResult?> AwardOrUpgradeBadgeAsync(int employeeId, Badge badge, string reason)
        {
            var existing = await _employeeBadgeRepo.GetOneAsync(
                eb => eb.EmployeeId == employeeId && eb.EarnedReason == reason);

            if (existing == null)
            {
                await _employeeBadgeRepo.CreateAsync(new EmployeeBadge
                {
                    EmployeeId = employeeId,
                    BadgeId = badge.Id,
                    EarnedReason = reason,
                    EarnedAt = DateTime.UtcNow
                });

                return new EmployeeBadgeAwardResult
                {
                    BadgeName = badge.Name,
                    Tier = badge.Tier,
                    PointsDelta = badge.Points
                };
            }

            if (existing.BadgeId == badge.Id)
            {
                return null;
            }

            var currentBadge = await _badgeRepo.GetOneAsync(b => b.Id == existing.BadgeId);
            var currentPoints = currentBadge?.Points ?? 0;
            if (badge.Points <= currentPoints)
            {
                return null;
            }

            existing.BadgeId = badge.Id;
            existing.EarnedAt = DateTime.UtcNow;
            await _employeeBadgeRepo.UpdateAsync(existing);

            return new EmployeeBadgeAwardResult
            {
                BadgeName = badge.Name,
                Tier = badge.Tier,
                PointsDelta = badge.Points - currentPoints
            };
        }

        private CourseSnapshot BuildCourseSnapshot(
            Course course,
            IReadOnlyCollection<EmployeeLesson> lessonRecords,
            IReadOnlyCollection<EmployeeExamAttempt> attempts)
        {
            var completedLessonIds = lessonRecords
                .Where(l => l.IsCompleted)
                .Select(l => l.LessonId)
                .ToHashSet();

            var publishedExams = course.Exams
                .Where(e => e.IsPublished)
                .OrderBy(e => e.CreatedAt)
                .ToList();

            var attemptsByExam = attempts
                .GroupBy(a => a.ExamId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.ScorePercentage).ToList());

            var snapshot = new CourseSnapshot();
            var sortSeed = 10;

            foreach (var lesson in course.Lessons
                .Where(l => !l.ChapterId.HasValue)
                .OrderBy(l => l.Order)
                .ThenBy(l => l.Title))
            {
                var lessonState = new LessonSnapshot
                {
                    Lesson = lesson,
                    IsUnlocked = true,
                    IsCompleted = completedLessonIds.Contains(lesson.Id),
                    SortKey = sortSeed++
                };

                if (snapshot.NextLessonId == null && lessonState.IsUnlocked && !lessonState.IsCompleted)
                {
                    snapshot.NextLessonId = lesson.Id;
                }

                snapshot.GeneralLessons.Add(lessonState);
                snapshot.AllLessons.Add(lessonState);
            }

            var previousChaptersCompleted = true;
            foreach (var chapter in course.Chapters
                .OrderBy(ch => ch.Order)
                .ThenBy(ch => ch.Title))
            {
                var chapterSnapshot = new ChapterSnapshot
                {
                    Chapter = chapter,
                    IsUnlocked = previousChaptersCompleted
                };

                var orderedLessons = course.Lessons
                    .Where(l => l.ChapterId == chapter.Id)
                    .OrderBy(l => l.Order)
                    .ThenBy(l => l.Title)
                    .ToList();

                var previousLessonsCompleted = true;
                foreach (var lesson in orderedLessons)
                {
                    var lessonState = new LessonSnapshot
                    {
                        Lesson = lesson,
                        IsUnlocked = chapterSnapshot.IsUnlocked && previousLessonsCompleted,
                        IsCompleted = completedLessonIds.Contains(lesson.Id),
                        SortKey = sortSeed++
                    };

                    if (snapshot.NextLessonId == null && lessonState.IsUnlocked && !lessonState.IsCompleted)
                    {
                        snapshot.NextLessonId = lesson.Id;
                    }

                    if (!lessonState.IsCompleted)
                    {
                        previousLessonsCompleted = false;
                    }

                    chapterSnapshot.Lessons.Add(lessonState);
                    snapshot.AllLessons.Add(lessonState);
                }

                var chapterExam = publishedExams
                    .Where(e => e.ChapterId == chapter.Id)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefault();

                var allChapterLessonsCompleted = chapterSnapshot.Lessons.All(l => l.IsCompleted);
                if (chapterExam != null)
                {
                    var examAttempts = attemptsByExam.TryGetValue(chapterExam.Id, out var examAttemptRows)
                        ? examAttemptRows
                        : new List<EmployeeExamAttempt>();

                    chapterSnapshot.Exam = new ExamSnapshot
                    {
                        Exam = chapterExam,
                        IsUnlocked = chapterSnapshot.IsUnlocked && allChapterLessonsCompleted,
                        IsPassed = examAttempts.Any(a => a.IsPassed),
                        BestScore = examAttempts.Any() ? examAttempts.Max(a => a.ScorePercentage) : null,
                        AttemptCount = examAttempts.Count,
                        SortKey = sortSeed++
                    };

                    snapshot.AllExams.Add(chapterSnapshot.Exam);
                }

                chapterSnapshot.IsCompleted = allChapterLessonsCompleted &&
                    (chapterSnapshot.Exam == null || chapterSnapshot.Exam.IsPassed);

                snapshot.Chapters.Add(chapterSnapshot);
                previousChaptersCompleted = chapterSnapshot.IsCompleted;
            }

            var allLessonsCompleted = snapshot.AllLessons.All(l => l.IsCompleted);
            var chaptersComplete = snapshot.Chapters.All(ch => ch.IsCompleted);

            foreach (var exam in publishedExams
                .Where(e => !e.ChapterId.HasValue)
                .OrderBy(e => e.CreatedAt))
            {
                var examAttempts = attemptsByExam.TryGetValue(exam.Id, out var examAttemptRows)
                    ? examAttemptRows
                    : new List<EmployeeExamAttempt>();

                var examSnapshot = new ExamSnapshot
                {
                    Exam = exam,
                    IsUnlocked = allLessonsCompleted && chaptersComplete,
                    IsPassed = examAttempts.Any(a => a.IsPassed),
                    BestScore = examAttempts.Any() ? examAttempts.Max(a => a.ScorePercentage) : null,
                    AttemptCount = examAttempts.Count,
                    SortKey = sortSeed++
                };

                snapshot.FinalAssessments.Add(examSnapshot);
                snapshot.AllExams.Add(examSnapshot);
            }

            snapshot.TotalLessons = snapshot.AllLessons.Count;
            snapshot.CompletedLessons = snapshot.AllLessons.Count(l => l.IsCompleted);

            var publishedExamCount = snapshot.AllExams.Count;
            var passedExamCount = snapshot.AllExams.Count(e => e.IsPassed);
            snapshot.TotalUnits = snapshot.TotalLessons + publishedExamCount;
            snapshot.CompletedUnits = snapshot.CompletedLessons + passedExamCount;
            snapshot.ProgressPercentage = snapshot.TotalUnits == 0
                ? 0
                : Math.Round(snapshot.CompletedUnits * 100d / snapshot.TotalUnits, 1);
            snapshot.IsCompleted = snapshot.TotalUnits > 0 &&
                snapshot.AllLessons.All(l => l.IsCompleted) &&
                snapshot.AllExams.All(e => e.IsPassed);
            snapshot.HasUnlockedExam = snapshot.AllExams.Any(e => e.IsUnlocked && !e.IsPassed);

            if (snapshot.AllExams.Any())
            {
                var bestExamScores = snapshot.AllExams
                    .Where(e => e.BestScore.HasValue)
                    .Select(e => e.BestScore!.Value)
                    .ToList();

                snapshot.FinalScore = bestExamScores.Any()
                    ? Math.Round(bestExamScores.Average(), 1)
                    : snapshot.IsCompleted ? 100 : null;
            }
            else
            {
                snapshot.FinalScore = snapshot.IsCompleted && snapshot.TotalLessons > 0 ? 100 : null;
            }

            return snapshot;
        }

        private static string MapStatus(bool isCompleted, double progress)
        {
            if (isCompleted) return "Completed";
            return progress > 0 ? "In Progress" : "Not Started";
        }

        private static string ResolveTier(double score)
        {
            if (score >= 90) return "Gold";
            if (score >= 75) return "Silver";
            return "Bronze";
        }

        private static string NormalizeOption(string? option)
        {
            var value = (option ?? string.Empty).Trim().ToUpperInvariant();
            return value is "A" or "B" or "C" or "D" ? value : string.Empty;
        }

        private static string BuildCertificateNumber(int courseId, int employeeId, DateTime completedAt)
        {
            return $"CERT-{courseId:D4}-{employeeId:D4}-{completedAt:yyyyMMdd}";
        }

        private static string HumanizeReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "Achievement unlocked";
            }

            if (reason.StartsWith("chapter-exam:", StringComparison.OrdinalIgnoreCase))
            {
                return "Passed a chapter exam";
            }

            if (reason.StartsWith("course-completion:", StringComparison.OrdinalIgnoreCase))
            {
                return "Completed a full course";
            }

            return reason;
        }

        private static ServiceResult<T> Ok<T>(T data, string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        private static ServiceResult<T> Fail<T>(string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message
            };
        }

        private sealed class EmployeeCourseContext
        {
            public EmployeeCourse Assignment { get; set; } = null!;
            public Course Course { get; set; } = null!;
            public List<EmployeeLesson> LessonRecords { get; set; } = new();
            public List<EmployeeExamAttempt> Attempts { get; set; } = new();
        }

        private sealed class CourseSyncResult
        {
            public bool CourseCompleted { get; set; }
            public bool CertificateAvailable { get; set; }
        }

        private sealed class EmployeeBadgeAwardResult
        {
            public string BadgeName { get; set; } = string.Empty;
            public string Tier { get; set; } = string.Empty;
            public int PointsDelta { get; set; }
        }

        private sealed class CourseSnapshot
        {
            public List<LessonSnapshot> GeneralLessons { get; } = new();
            public List<ChapterSnapshot> Chapters { get; } = new();
            public List<ExamSnapshot> FinalAssessments { get; } = new();
            public List<LessonSnapshot> AllLessons { get; } = new();
            public List<ExamSnapshot> AllExams { get; } = new();
            public int TotalLessons { get; set; }
            public int CompletedLessons { get; set; }
            public int TotalUnits { get; set; }
            public int CompletedUnits { get; set; }
            public double ProgressPercentage { get; set; }
            public bool IsCompleted { get; set; }
            public bool HasUnlockedExam { get; set; }
            public int? NextLessonId { get; set; }
            public double? FinalScore { get; set; }
        }

        private sealed class ChapterSnapshot
        {
            public CourseChapter Chapter { get; set; } = null!;
            public bool IsUnlocked { get; set; }
            public bool IsCompleted { get; set; }
            public List<LessonSnapshot> Lessons { get; } = new();
            public ExamSnapshot? Exam { get; set; }
        }

        private sealed class LessonSnapshot
        {
            public Lesson Lesson { get; set; } = null!;
            public bool IsUnlocked { get; set; }
            public bool IsCompleted { get; set; }
            public int SortKey { get; set; }
        }

        private sealed class ExamSnapshot
        {
            public Exam Exam { get; set; } = null!;
            public bool IsUnlocked { get; set; }
            public bool IsPassed { get; set; }
            public double? BestScore { get; set; }
            public int AttemptCount { get; set; }
            public int SortKey { get; set; }
        }
    }
}
