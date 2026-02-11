

namespace Traninig_Managment_system.DAL.Model
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email{ get; set; }
        public string JobTitle { get; set; } = string.Empty;

        public double Points { get; set; }

        public bool IsActive { get; set; } = true;

        // 🔗 الربط مع ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;

        public ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();
        public ICollection<EmployeeBadge> EmployeeBadges { get; set; } = new List<EmployeeBadge>();
        public ICollection<EmployeeLesson> EmployeeLessons { get; set; }


    }
}
