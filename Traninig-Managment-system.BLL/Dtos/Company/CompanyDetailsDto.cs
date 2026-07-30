namespace Traninig_Managment_system.BLL.Dtos.Company
{
    /// <summary>
    /// بترجع في صفحة التفاصيل (View Model مفصلة)
    /// </summary>
    public class CompanyDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public bool IsActive { get; set; }
        public DateTime SubscriptionStart { get; set; }
        public DateTime SubscriptionEnd { get; set; }

        // Plan Info
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int MaxEmployees { get; set; }
        public int MaxCourses { get; set; }

        // Stats
        public int TotalEmployees { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
    }
}
