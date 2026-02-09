using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CourseVm
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string InstructorName { get; set; } 
        public string logoUrl { get; set; }
        public int DurationInHours { get; set; }


    }
}
