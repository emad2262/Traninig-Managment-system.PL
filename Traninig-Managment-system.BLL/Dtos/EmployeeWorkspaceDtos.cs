namespace Traninig_Managment_system.BLL.Dtos
{
    public class EmployeeDashboardVm
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public double AverageProgress { get; set; }
        public double TotalPoints { get; set; }
        public int TotalCompletedLessons { get; set; }
        public int CertificatesCount { get; set; }
        public List<EmployeeCourseCardVm> Courses { get; set; } = new();
        public List<EmployeeEarnedBadgeVm> Badges { get; set; } = new();
        public List<EmployeeActivityItemVm> RecentActivity { get; set; } = new();
    }

    public class EmployeeCourseCardVm
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool CertificateAvailable { get; set; }
        public bool CertificatePending { get; set; }
        public string CertificateStatusText { get; set; } = string.Empty;
        public int? NextLessonId { get; set; }
        public string? Highlight { get; set; }
    }

    public class EmployeeEarnedBadgeVm
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BadgeType { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? IconUrl { get; set; }
        public DateTime EarnedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class EmployeeActivityItemVm
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime HappenedAt { get; set; }
        public string Kind { get; set; } = string.Empty;
    }

    public class EmployeeCourseDetailsVm
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public double? FinalScore { get; set; }
        public bool CertificateAvailable { get; set; }
        public bool CertificatePending { get; set; }
        public string CertificateStatusText { get; set; } = string.Empty;
        public int? NextLessonId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<EmployeeChapterProgressVm> Chapters { get; set; } = new();
        public List<EmployeeLessonRowVm> GeneralLessons { get; set; } = new();
        public List<EmployeeExamSummaryVm> FinalAssessments { get; set; } = new();
        public List<EmployeeEarnedBadgeVm> EarnedBadges { get; set; } = new();
    }

    public class EmployeeChapterProgressVm
    {
        public int ChapterId { get; set; }
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public List<EmployeeLessonRowVm> Lessons { get; set; } = new();
        public EmployeeExamSummaryVm? Exam { get; set; }
    }

    public class EmployeeLessonRowVm
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasVideo { get; set; }
        public bool HasPdf { get; set; }
        public string? ChapterTitle { get; set; }
    }

    public class EmployeeExamSummaryVm
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PassingScore { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsPassed { get; set; }
        public double? BestScore { get; set; }
        public int AttemptCount { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? EarnedTier { get; set; }
    }

    public class EmployeeLessonWatchVm
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ChapterTitle { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string PdfUrl { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsUnlocked { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public double CourseProgressPercentage { get; set; }
        public int? PreviousLessonId { get; set; }
        public int? NextLessonId { get; set; }
    }

    public class EmployeeLessonCompletionResultVm
    {
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public int? NextLessonId { get; set; }
        public bool CourseCompleted { get; set; }
        public bool CertificateAvailable { get; set; }
        public bool CertificatePending { get; set; }
        public string CertificateStatusText { get; set; } = string.Empty;
    }

    public class EmployeeExamTakeVm
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public List<EmployeeExamQuestionVm> Questions { get; set; } = new();
    }

    public class EmployeeExamQuestionVm
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class EmployeeExamSubmissionVm
    {
        public int ExamId { get; set; }
        public List<EmployeeExamAnswerVm> Answers { get; set; } = new();
    }

    public class EmployeeExamAnswerVm
    {
        public int QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty;
    }

    public class EmployeeExamResultVm
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string ExamTitle { get; set; } = string.Empty;
        public string? ChapterTitle { get; set; }
        public double ScorePercentage { get; set; }
        public int PassingScore { get; set; }
        public bool IsPassed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int AttemptCount { get; set; }
        public bool CertificateAvailable { get; set; }
        public bool CertificatePending { get; set; }
        public string CertificateStatusText { get; set; } = string.Empty;
        public bool CourseCompleted { get; set; }
        public string? AwardedBadgeName { get; set; }
        public string? AwardedBadgeTier { get; set; }
    }

    public class EmployeeCertificateVm
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public double? FinalScore { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
    }
}
