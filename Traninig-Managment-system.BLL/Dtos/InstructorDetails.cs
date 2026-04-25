using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class InstructorDetails
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? FullName { get; set; } 

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Specialization { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CourseDto> Courses { get; set; } = new List<CourseDto>();
    }
}
