using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class AssignEmployeeCoursesVm
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public List<CourseAssignItemVm> Courses { get; set; } = new();
    }


}
