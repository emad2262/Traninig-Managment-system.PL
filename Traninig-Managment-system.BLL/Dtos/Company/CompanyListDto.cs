namespace Traninig_Managment_system.BLL.Dtos.Company
{
    /// <summary>
    /// بترجع في قايمة الشركات (View Model بسيطة)
    /// </summary>
    public class CompanyListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public bool IsActive { get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public string PlanName { get; set; } = string.Empty;
    }
}
