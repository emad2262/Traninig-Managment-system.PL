using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class instructorIndexVm
    {
        public IEnumerable<ListInstructorVm> Instructors { get; set; } = new List<ListInstructorVm>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Name { get; set; }
    }
}
