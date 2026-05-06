namespace Traninig_Managment_system.BLL.Services.classes
{
    public abstract class EmployeeWorkspaceServiceBase
    {
        protected const double LessonCompletionPoints = 10;

        protected readonly IEmployeeRepo EmployeeRepo;
        protected readonly IEmployeeCourseRepo EmployeeCourseRepo;
        protected readonly ICourseRepo CourseRepo;
        protected readonly ILessonRepo LessonRepo;
        protected readonly IEmployeeLessonRepo EmployeeLessonRepo;
        protected readonly IExamRepo ExamRepo;
        protected readonly IEmployeeExamAttemptRepo EmployeeExamAttemptRepo;
        protected readonly IEmployeeBadgeRepo EmployeeBadgeRepo;
        protected readonly IBadgeRepo BadgeRepo;
        protected readonly ICompanyRepo CompanyRepo;
        protected readonly IEmployeeCertificateRepo CertificateRepo;
        protected readonly ICompanyNotificationRepo CompanyNotificationRepo;

        protected EmployeeWorkspaceServiceBase(
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
        {
            EmployeeRepo = employeeRepo;
            EmployeeCourseRepo = employeeCourseRepo;
            CourseRepo = courseRepo;
            LessonRepo = lessonRepo;
            EmployeeLessonRepo = employeeLessonRepo;
            ExamRepo = examRepo;
            EmployeeExamAttemptRepo = employeeExamAttemptRepo;
            EmployeeBadgeRepo = employeeBadgeRepo;
            BadgeRepo = badgeRepo;
            CompanyRepo = companyRepo;
            CertificateRepo = certificateRepo;
            CompanyNotificationRepo = companyNotificationRepo;
        }

        protected async Task<Employee?> ResolveEmployeeAsync(string userId)
        {
            return await EmployeeRepo.GetOneAsync(e => e.UserId == userId && e.IsActive);
        }

        protected async Task<EmployeeCourseContext?> LoadCourseContextAsync(int employeeId, int courseId)
        {
            var assignment = await EmployeeCourseRepo.GetOneAsync(
                ec => ec.EmployeeId == employeeId && ec.CourseId == courseId);
            if (assignment == null) return null;

            var course = await CourseRepo.GetOneAsync(
                c => c.Id == courseId,
                c => c.Category,
                c => c.Instructor!,
                c => c.Chapters,
                c => c.Lessons,
                c => c.Exams);
            if (course == null) return null;

            var lessonRecords = (await EmployeeLessonRepo.GetAllAsync(
                el => el.EmployeeId == employeeId && el.Lesson.CourseId == courseId)).ToList();

            var attempts = (await EmployeeExamAttemptRepo.GetAllAsync(
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

        protected async Task<List<EmployeeEarnedBadgeVm>> GetEmployeeBadgesAsync(int employeeId)
        {
            var employeeBadges = (await EmployeeBadgeRepo.GetAllAsync(
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

        protected List<EmployeeActivityItemVm> BuildActivities(
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

        protected EmployeeLessonRowVm MapLessonRow(LessonSnapshot lesson, string? chapterTitle = null)
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

        protected EmployeeExamSummaryVm MapExamSummary(ExamSnapshot exam)
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

        protected async Task<CourseSyncResult> PersistCourseStateAsync(
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

            await EmployeeCourseRepo.UpdateAsync(assignment);

            var totalPointsDelta = extraPoints;
            EmployeeCertificate? certificate = null;
            if (snapshot.IsCompleted)
            {
                var courseBadge = await AwardCourseCompletionBadgeAsync(employee.Id, course);
                totalPointsDelta += courseBadge?.PointsDelta ?? 0;
                certificate = await EnsureCertificateRequestAsync(employee, course, assignment, snapshot);
            }

            if (totalPointsDelta > 0)
            {
                employee.Points += totalPointsDelta;
                await EmployeeRepo.UpdateAsync(employee);
            }

            return new CourseSyncResult
            {
                CourseCompleted = snapshot.IsCompleted,
                CertificateIssued = certificate?.Status == CertificateStatus.Issued,
                CertificatePending = certificate?.Status == CertificateStatus.PendingCompanyApproval,
                CertificateStatusText = ResolveCertificateStatusText(certificate, snapshot.IsCompleted)
            };
        }

        protected async Task<EmployeeCertificate?> GetCertificateStateAsync(int employeeId, int courseId)
        {
            return await CertificateRepo.GetByEmployeeCourseAsync(employeeId, courseId);
        }

        protected async Task<EmployeeCertificate?> EnsureCertificateRequestAsync(
            Employee employee,
            Course course,
            EmployeeCourse assignment,
            CourseSnapshot snapshot)
        {
            var completedAt = assignment.CompletedAt ?? DateTime.UtcNow;
            var certificateNumber = BuildCertificateNumber(course.Id, employee.Id, completedAt);
            var existing = await CertificateRepo.GetForUpdateByEmployeeCourseAsync(employee.Id, course.Id);

            if (existing != null)
            {
                if (existing.Status == CertificateStatus.PendingCompanyApproval)
                {
                    existing.CompletedAt = completedAt;
                    existing.FinalScore = snapshot.FinalScore;
                    existing.CertificateNumber = string.IsNullOrWhiteSpace(existing.CertificateNumber)
                        ? certificateNumber
                        : existing.CertificateNumber;
                    await CertificateRepo.UpdateAsync(existing);
                }

                return existing;
            }

            var certificate = new EmployeeCertificate
            {
                CompanyId = employee.CompanyId,
                EmployeeId = employee.Id,
                CourseId = course.Id,
                CertificateNumber = certificateNumber,
                Status = CertificateStatus.PendingCompanyApproval,
                RequestedAt = DateTime.UtcNow,
                CompletedAt = completedAt,
                FinalScore = snapshot.FinalScore
            };

            await CertificateRepo.CreateAsync(certificate);
            await CompanyNotificationRepo.CreateAsync(new CompanyNotification
            {
                CompanyId = employee.CompanyId,
                Title = "Certificate approval needed",
                Message = $"{employee.Name} completed {course.Title}. Please review and issue the certificate from the company dashboard.",
                Type = CompanyNotificationType.CertificateRequest,
                DeliveryChannel = "Dashboard",
                ReferenceType = "Certificate",
                ReferenceId = certificate.Id,
                IsSent = true,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            return certificate;
        }

        protected async Task<EmployeeBadgeAwardResult?> AwardChapterExamBadgeAsync(int employeeId, Exam exam, double score)
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

        protected async Task<EmployeeBadgeAwardResult?> AwardCourseCompletionBadgeAsync(int employeeId, Course course)
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

        protected async Task<Badge> EnsureBadgeAsync(
            string name,
            string description,
            string badgeType,
            string tier,
            int points)
        {
            var badge = await BadgeRepo.GetOneAsync(b => b.Name == name);
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

            await BadgeRepo.CreateAsync(created);
            return created;
        }

        protected async Task<EmployeeBadgeAwardResult?> AwardOrUpgradeBadgeAsync(int employeeId, Badge badge, string reason)
        {
            var existing = await EmployeeBadgeRepo.GetOneAsync(
                eb => eb.EmployeeId == employeeId && eb.EarnedReason == reason);

            if (existing == null)
            {
                await EmployeeBadgeRepo.CreateAsync(new EmployeeBadge
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

            var currentBadge = await BadgeRepo.GetOneAsync(b => b.Id == existing.BadgeId);
            var currentPoints = currentBadge?.Points ?? 0;
            if (badge.Points <= currentPoints)
            {
                return null;
            }

            existing.BadgeId = badge.Id;
            existing.EarnedAt = DateTime.UtcNow;
            await EmployeeBadgeRepo.UpdateAsync(existing);

            return new EmployeeBadgeAwardResult
            {
                BadgeName = badge.Name,
                Tier = badge.Tier,
                PointsDelta = badge.Points - currentPoints
            };
        }

        protected CourseSnapshot BuildCourseSnapshot(
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

        protected static string MapStatus(bool isCompleted, double progress)
        {
            if (isCompleted) return "Completed";
            return progress > 0 ? "In Progress" : "Not Started";
        }

        protected static string ResolveTier(double score)
        {
            if (score >= 90) return "Gold";
            if (score >= 75) return "Silver";
            return "Bronze";
        }

        protected static string NormalizeOption(string? option)
        {
            var value = (option ?? string.Empty).Trim().ToUpperInvariant();
            return value is "A" or "B" or "C" or "D" ? value : string.Empty;
        }

        protected static string BuildCertificateNumber(int courseId, int employeeId, DateTime completedAt)
        {
            return $"CERT-{courseId:D4}-{employeeId:D4}-{completedAt:yyyyMMdd}";
        }

        protected static string ResolveCertificateStatusText(EmployeeCertificate? certificate, bool courseCompleted)
        {
            if (!courseCompleted)
            {
                return string.Empty;
            }

            return certificate?.Status switch
            {
                CertificateStatus.Issued => "Certificate issued by company",
                CertificateStatus.Revoked => "Certificate revoked",
                _ => "Waiting for company approval"
            };
        }

        protected static string HumanizeReason(string? reason)
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

        protected static ServiceResult<T> Ok<T>(T data, string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        protected static ServiceResult<T> Fail<T>(string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message
            };
        }

        protected sealed class EmployeeCourseContext
        {
            public EmployeeCourse Assignment { get; set; } = null!;
            public Course Course { get; set; } = null!;
            public List<EmployeeLesson> LessonRecords { get; set; } = new();
            public List<EmployeeExamAttempt> Attempts { get; set; } = new();
        }

        protected sealed class CourseSyncResult
        {
            public bool CourseCompleted { get; set; }
            public bool CertificateIssued { get; set; }
            public bool CertificatePending { get; set; }
            public string CertificateStatusText { get; set; } = string.Empty;
        }

        protected sealed class EmployeeBadgeAwardResult
        {
            public string BadgeName { get; set; } = string.Empty;
            public string Tier { get; set; } = string.Empty;
            public int PointsDelta { get; set; }
        }

        protected sealed class CourseSnapshot
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

        protected sealed class ChapterSnapshot
        {
            public CourseChapter Chapter { get; set; } = null!;
            public bool IsUnlocked { get; set; }
            public bool IsCompleted { get; set; }
            public List<LessonSnapshot> Lessons { get; } = new();
            public ExamSnapshot? Exam { get; set; }
        }

        protected sealed class LessonSnapshot
        {
            public Lesson Lesson { get; set; } = null!;
            public bool IsUnlocked { get; set; }
            public bool IsCompleted { get; set; }
            public int SortKey { get; set; }
        }

        protected sealed class ExamSnapshot
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
