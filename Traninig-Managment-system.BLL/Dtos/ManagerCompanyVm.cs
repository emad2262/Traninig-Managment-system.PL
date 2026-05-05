namespace Traninig_Managment_system.BLL.Dtos
{
    public class ManagerCompanyVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public int InstructorCount { get; set; }
        public int CategoryCount { get; set; }
        public int NotificationCount { get; set; }
        public bool ExpiringSoon { get; set; }
        public int DaysToRenewal { get; set; }
        public DateTime SubscriptionStart { get; set; }
        public DateTime SubscriptionEnd { get; set; }
    }

    public class ManagerCompanyDetailsVm : ManagerCompanyVm
    {
        public List<ManagerNotificationVm> Notifications { get; set; } = new();
        public List<PlanFeatureVm> PlanFeatures { get; set; } = new();
        public CreateCompanyNotificationVm NotificationForm { get; set; } = new();
    }

    public class ManagerNotificationVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public CompanyNotificationType Type { get; set; } = CompanyNotificationType.General;
        public string DeliveryChannel { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

    public class CreateCompanyNotificationVm
    {
        public int CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public CompanyNotificationType Type { get; set; } = CompanyNotificationType.General;
        public string DeliveryChannel { get; set; } = "Dashboard";
    }
}
