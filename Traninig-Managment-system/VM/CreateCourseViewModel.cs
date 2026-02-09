using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.VM
{
    public class CreateCourseViewModel
    {
        [Required]
        public string CourseName { get; set; }

        public string Description { get; set; }

        public int DurationInHours { get; set; }

        public IFormFile? Logo { get; set; }   

        public int? InstructorId { get; set; }
    }
}
