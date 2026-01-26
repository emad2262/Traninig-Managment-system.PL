using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class PlanCreateVm
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int DurationInDays { get; set; }
        public int MaxCourses { get; set; }
        public int MaxEmployees { get; set; }
        public bool IsActive { get; set; }
    }
}
