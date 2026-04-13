
namespace Traninig_Managment_system.DAL.Model
{
    public enum CourseStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
    public class EmployeeCourse
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime AssignedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; } 
        public DateTime? CompletedAt { get; set; }

        public CourseStatus Status { get; set; }

        public double Progress { get; set; } 
        public double? FinalScore { get; set; } 
       
    }
}
