namespace Traninig_Managment_system.DAL.Model
{
    public class EmployeeBadge
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public  Employee Employee { get; set; } = null!;

        public int BadgeId { get; set; }

        [ForeignKey(nameof(BadgeId))]
        public  Badge Badge { get; set; } = null!;

        public DateTime EarnedAt { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? EarnedReason { get; set; } // "Completed 5 courses", "First login"
    }
}