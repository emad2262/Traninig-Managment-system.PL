using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Course
{
    public class ListCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public int LessonCount { get; set; }
        public bool IsPublished { get; set; }

       
    }
}
