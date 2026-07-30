using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Course
{
    public class UpdateCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public bool IsPublish { get; set;  }
       
    }
}
