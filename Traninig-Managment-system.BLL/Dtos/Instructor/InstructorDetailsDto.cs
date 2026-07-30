using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Instructor
{
    public class InstructorDetailsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Specialization { get; set; }
        public DateTime CreateAt { get; set; }
        public int TotalCourses { get; set; }
        public string? ProfileImage { get; set; }
    }
}
