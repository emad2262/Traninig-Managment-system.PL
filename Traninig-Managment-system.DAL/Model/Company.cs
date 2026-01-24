using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.DAL.Model
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime SubscriptionStart { get; set; }

        public DateTime SubscriptionEnd { get; set; }

        public bool IsActive { get; set; } = true;

        public int PlanId { get; set; }
        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; }

        public virtual ICollection<AdminCompany> Admins { get; set; } = new List<AdminCompany>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Courses> Courses { get; set; } = new List<Courses>();
    }
}
