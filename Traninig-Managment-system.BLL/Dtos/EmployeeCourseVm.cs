using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class EmployeeCourseVm
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string InstructorName { get; set; }
        public double Progress { get; set; }
        public CourseStatus Status { get; set; }
        public double? FinalScore { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; } = DateTime.MinValue;
    }
}
