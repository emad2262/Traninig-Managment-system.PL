using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? Logo { get; set; }

        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CategoryId { get; set; }
        public int? InstructorId { get; set; }

        public string? CategoryName { get; set; }
        public string? InstructorName { get; set; }
    }
}
