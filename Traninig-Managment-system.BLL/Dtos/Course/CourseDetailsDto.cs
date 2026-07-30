using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Traninig_Managment_system.BLL.Dtos.Lessons;

namespace Traninig_Managment_system.BLL.Dtos.Course
{
    public class CourseDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? InstructorName { get; set; }
        public string? CategoryName { get; set; }
        public ICollection<LessonListDto> LessonsList { get; set; } = new List<LessonListDto>();
        
    }
}
