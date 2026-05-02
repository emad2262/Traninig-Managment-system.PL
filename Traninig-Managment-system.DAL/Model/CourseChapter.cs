namespace Traninig_Managment_system.DAL.Model
{
    public class CourseChapter
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(180)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    }
}
