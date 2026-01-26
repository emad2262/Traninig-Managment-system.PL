using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class DashboardVm
    {
        public int TotalCompanies { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalCourses { get; set; }
        public int ExpiringSoon { get; set; }
        public double totalpage { get; set; }
        public List<Company> companyList { get; set; }
    }
}
