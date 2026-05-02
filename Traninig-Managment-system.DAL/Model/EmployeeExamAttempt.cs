namespace Traninig_Managment_system.DAL.Model
{
    public class EmployeeExamAttempt
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;

        public int ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public Exam Exam { get; set; } = null!;

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public double ScorePercentage { get; set; }

        public bool IsPassed { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
