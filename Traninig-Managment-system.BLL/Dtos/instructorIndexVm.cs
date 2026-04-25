using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class instructorIndexVm
    {
        public IEnumerable<ListInstructorVm> Instructors { get; set; } = new List<ListInstructorVm>();
        public string Name { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int TotalPages { get; set; }
    }
}
