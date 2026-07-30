using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.BLL.Dtos.Company
{
    /// <summary>
    /// DTO لتعديل بيانات الشركة
    /// </summary>
    public class UpdateCompanyDto
    {
        [Required]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public string? Logo { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public DateTime SubscriptionStart { get; set; }

        [Required]
        public DateTime SubscriptionEnd { get; set; }
    }
}
