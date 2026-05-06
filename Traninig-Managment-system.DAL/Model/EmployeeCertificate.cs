namespace Traninig_Managment_system.DAL.Model
{
    public enum CertificateStatus
    {
        PendingCompanyApproval = 1,
        Issued = 2,
        Revoked = 3
    }

    public class EmployeeCertificate
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        [Required, MaxLength(80)]
        public string CertificateNumber { get; set; } = string.Empty;

        public CertificateStatus Status { get; set; } = CertificateStatus.PendingCompanyApproval;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public DateTime? IssuedAt { get; set; }

        public DateTime? SentAt { get; set; }

        [MaxLength(450)]
        public string? IssuedByUserId { get; set; }

        [MaxLength(1000)]
        public string? CompanyNotes { get; set; }

        public double? FinalScore { get; set; }
    }
}
