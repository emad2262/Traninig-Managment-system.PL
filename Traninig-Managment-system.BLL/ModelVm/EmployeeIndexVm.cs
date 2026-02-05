using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class EmployeeIndexVm
    {

        public IEnumerable<ListEmployeeVm> ListEmployees { get; set; } = new List<ListEmployeeVm>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string? Name { get; set; }
    }
}
