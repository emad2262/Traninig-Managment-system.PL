namespace Traninig_Managment_system.BLL.Dtos
{
    public enum ActivityType
    {
        CourseCreated,
        CourseAssigned,
        CourseCompleted,
        BadgeEarned,
        CertificateRequested,
        CertificateIssued
    }

    public class ActivityTimelineVm
    {
        public ActivityType ActivityType { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string ActorRole { get; set; } = "Employee";
        public string ActionText { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string ContextName { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }
}
