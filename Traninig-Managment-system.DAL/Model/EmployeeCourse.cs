using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Model
{
    public class EmployeeCourse
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int CourseId { get; set; }
        public Courses Course { get; set; }

        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public double PointsEarned { get; set; }
    }
}
