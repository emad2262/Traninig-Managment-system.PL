
namespace Traninig_Managment_system.DAL.Model
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]

        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }

        public DateTime SubscriptionStart { get; set; }

        public DateTime SubscriptionEnd { get; set; }

        public bool IsActive { get; set; } = true;

        public int PlanId { get; set; }
        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; }

        public  ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
        public  ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<CourseCategory> CoursesCategories { get; set;} = new List<CourseCategory>();

    }
}
