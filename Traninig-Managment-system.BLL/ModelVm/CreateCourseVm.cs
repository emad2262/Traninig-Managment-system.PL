using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CreateCourseVm
    {
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; }
        public int DurationInHours { get; set; }
        public int? InstructorId { get; set; }
        public string? LogoPath { get; set; }
    }
}
