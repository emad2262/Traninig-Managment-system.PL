namespace Traninig_Managment_system.BLL.Dtos
{
    public class CompanyCertificateListItemVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public CertificateStatus Status { get; set; }
        public double? FinalScore { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

    public class CompanyCertificateDetailsVm : CompanyCertificateListItemVm
    {
        public string CompanyName { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public string? CompanyNotes { get; set; }
    }

    public class CompanyCertificateIssueVm
    {
        public int CertificateId { get; set; }

        [MaxLength(1000)]
        public string? CompanyNotes { get; set; }

        public bool SendEmail { get; set; } = true;
    }
}
