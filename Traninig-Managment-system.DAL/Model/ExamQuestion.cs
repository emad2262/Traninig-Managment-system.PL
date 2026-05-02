namespace Traninig_Managment_system.DAL.Model
{
    public class ExamQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string OptionA { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string OptionB { get; set; } = string.Empty;

        [MaxLength(250)]
        public string OptionC { get; set; } = string.Empty;

        [MaxLength(250)]
        public string OptionD { get; set; } = string.Empty;

        [Required, MaxLength(1)]
        public string CorrectOption { get; set; } = "A";

        public int Points { get; set; } = 1;

        public int ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public Exam Exam { get; set; } = null!;
    }
}
