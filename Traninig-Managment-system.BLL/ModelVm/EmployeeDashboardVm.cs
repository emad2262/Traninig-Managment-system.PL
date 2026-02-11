using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class EmployeeDashboardVm
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalCompletedLessons { get; set; }
        public double TotalPoints { get; set; }

        public List<CourseProgressVm> Courses { get; set; }
    }
}
