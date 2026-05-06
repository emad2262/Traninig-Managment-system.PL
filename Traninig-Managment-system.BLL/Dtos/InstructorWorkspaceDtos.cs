using Microsoft.AspNetCore.Http;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class InstructorDashboardVm
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int TotalChapters { get; set; }
        public int TotalLessons { get; set; }
        public int TotalExams { get; set; }
        public int TotalAssignedEmployees { get; set; }
        public double AverageProgress { get; set; }
        public List<InstructorCategoryVm> Categories { get; set; } = new();
        public List<InstructorCourseCardVm> Courses { get; set; } = new();
        public List<InstructorEmployeeProgressVm> RecentProgress { get; set; } = new();
    }

    public class InstructorCategoryVm
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<InstructorCourseCardVm> Courses { get; set; } = new();
    }

    public class InstructorCourseCardVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public int DurationInHours { get; set; }
        public int ChapterCount { get; set; }
        public int LessonCount { get; set; }
        public int ExamCount { get; set; }
        public int AssignedEmployees { get; set; }
        public double AverageProgress { get; set; }
    }

    public class InstructorCourseDetailsVm : InstructorCourseCardVm
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<InstructorChapterVm> Chapters { get; set; } = new();
        public List<InstructorLessonVm> Lessons { get; set; } = new();
        public List<InstructorEmployeeProgressVm> Employees { get; set; } = new();
        public List<InstructorExamVm> Exams { get; set; } = new();
    }

    public class InstructorLessonVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ContentUrl { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CompletedEmployees { get; set; }
        public double AverageWatchedPercentage { get; set; }
    }

    public class InstructorLessonFormVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ChapterId { get; set; }
        public List<InstructorChapterOptionVm> ChapterOptions { get; set; } = new();

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        [StringLength(180, ErrorMessage = "عنوان الدرس لا يجب أن يتخطى 180 حرف")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "وصف الدرس")]
        [StringLength(1500, ErrorMessage = "وصف الدرس لا يجب أن يتخطى 1500 حرف")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 999, ErrorMessage = "ترتيب الدرس يجب أن يكون أكبر من صفر")]
        public int Order { get; set; } = 1;

        public string? ExistingContentUrl { get; set; }
        public IFormFile? PdfFile { get; set; }
        public IFormFile? VideoFile { get; set; }
    }

    public class InstructorChapterVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LessonCount { get; set; }
        public int ExamCount { get; set; }
        public double AverageProgress { get; set; }
        public List<InstructorLessonVm> Lessons { get; set; } = new();
        public List<InstructorExamVm> Exams { get; set; } = new();
    }

    public class InstructorChapterFormVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        [Required(ErrorMessage = "عنوان الشابتر مطلوب")]
        [StringLength(180, ErrorMessage = "عنوان الشابتر لا يجب أن يتخطى 180 حرف")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "وصف الشابتر لا يجب أن يتخطى 1000 حرف")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 999, ErrorMessage = "ترتيب الشابتر يجب أن يكون أكبر من صفر")]
        public int Order { get; set; } = 1;
    }

    public class InstructorChapterOptionVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class InstructorEmployeeProgressVm
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? FinalScore { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int BadgeCount { get; set; }
    }

    public class InstructorEmployeeDetailsVm
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public double Points { get; set; }
        public List<InstructorEmployeeProgressVm> Courses { get; set; } = new();
        public List<InstructorBadgeVm> Badges { get; set; } = new();
    }

    public class InstructorBadgeVm
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public int Points { get; set; }
        public DateTime EarnedAt { get; set; }
    }

    public class InstructorExamVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QuestionCount { get; set; }
    }

    public class InstructorExamFormVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ChapterId { get; set; }
        public string? ChapterTitle { get; set; }
        public List<InstructorChapterOptionVm> ChapterOptions { get; set; } = new();

        [Required(ErrorMessage = "عنوان الامتحان مطلوب")]
        [StringLength(180, ErrorMessage = "عنوان الامتحان لا يجب أن يتخطى 180 حرف")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "وصف الامتحان لا يجب أن يتخطى 1000 حرف")]
        public string Description { get; set; } = string.Empty;

        [Range(5, 300, ErrorMessage = "مدة الامتحان يجب أن تكون بين 5 و 300 دقيقة")]
        public int DurationMinutes { get; set; } = 30;

        [Range(1, 100, ErrorMessage = "درجة النجاح يجب أن تكون بين 1 و 100")]
        public int PassingScore { get; set; } = 60;

        public bool IsPublished { get; set; }
        public List<InstructorExamQuestionFormVm> Questions { get; set; } = new();
    }

    public class InstructorExamQuestionFormVm
    {
        public int Id { get; set; }

        [StringLength(500, ErrorMessage = "نص السؤال لا يجب أن يتخطى 500 حرف")]
        public string Text { get; set; } = string.Empty;

        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = "A";
        public int Points { get; set; } = 1;
    }
}
