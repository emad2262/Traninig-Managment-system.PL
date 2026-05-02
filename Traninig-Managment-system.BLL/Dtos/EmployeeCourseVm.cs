using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class EmployeeCourseVm
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
        public bool IsAssigned { get; set; }
        public double Progress { get; set; }
        public CourseStatus Status { get; set; }
        public double? FinalScore { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; } = DateTime.MinValue;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
