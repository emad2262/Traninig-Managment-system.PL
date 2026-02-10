using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class InstractorDashVm
    {
        // Summary Cards
        public int TotalCategories { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEmployees { get; set; }

        // Page Data
        public List<CategoryVm> Categories { get; set; } = new();
        public List<ListEmployeeVm> Employees { get; set; } = new();
    }
}
