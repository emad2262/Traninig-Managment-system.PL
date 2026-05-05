namespace Traninig_Managment_system.DAL.Model
{
    public enum CompanyNotificationType
    {
        General = 1,
        RenewalReminder = 2,
        FeatureUpdate = 3,
        Billing = 4
    }

    public class CompanyNotification
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public CompanyNotificationType Type { get; set; } = CompanyNotificationType.General;

        [MaxLength(60)]
        public string DeliveryChannel { get; set; } = "Dashboard";

        public bool IsSent { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; } = DateTime.UtcNow;
    }
}
