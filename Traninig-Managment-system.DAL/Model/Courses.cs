namespace Traninig_Managment_system.DAL.Model;
public class Courses
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string logo { get; set; } = string.Empty;
    public int DurationInHours { get; set; }

    public bool IsPublished { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
    public int? InstructorId { get; set; }

    [ForeignKey(nameof(InstructorId))]
    public virtual Instructor? Instructor { get; set; }

    // ✅ Category (One-to-Many)
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public CourseCategory Category { get; set; } = null!;

    // Navigation Properties
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();

}
