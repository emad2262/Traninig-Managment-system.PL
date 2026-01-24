
namespace Traninig_Managment_system.DAL.Model
{
    public class Courses
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;
        public  ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();

    }
}
