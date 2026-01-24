

namespace Traninig_Managment_system.DAL.Model
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        public double Points { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();
    }
}
