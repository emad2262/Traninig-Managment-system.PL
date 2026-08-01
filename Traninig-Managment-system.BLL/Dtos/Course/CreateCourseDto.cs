using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Course
{
    public class CreateCourseDto
    {
       
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CategoryId { get; set; }
        public int InstructorId { get; set; }

    }
}
