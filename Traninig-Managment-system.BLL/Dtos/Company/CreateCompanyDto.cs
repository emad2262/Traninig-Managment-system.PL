using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.BLL.Dtos.Company
{
    /// <summary>
    /// DTO لإنشاء شركة جديدة
    /// </summary>
    public class CreateCompanyDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public string? Logo { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public DateTime SubscriptionStart { get; set; }

        [Required]
        public DateTime SubscriptionEnd { get; set; }
    }
}
