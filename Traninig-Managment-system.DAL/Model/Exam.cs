namespace Traninig_Managment_system.DAL.Model
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(180)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; } = 30;

        public int PassingScore { get; set; } = 60;

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        public int? ChapterId { get; set; }

        [ForeignKey(nameof(ChapterId))]
        public CourseChapter? Chapter { get; set; }

        public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    }
}
