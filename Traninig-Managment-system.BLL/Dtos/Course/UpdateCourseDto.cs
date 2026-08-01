using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.BLL.Dtos.Course;
using Traninig_Managment_system.DAL.Repo;

namespace Traninig_Managment_system.BLL.Dtos.Course
{
    public class UpdateCourseDto
    {
        public int Id { get; set; }
       
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Logo { get; set; } 
        public int DurationInHours { get; set; }
        public bool IsPublish { get; set;  }
        public int CategoryId { get; set; }
        public int? InstructorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
