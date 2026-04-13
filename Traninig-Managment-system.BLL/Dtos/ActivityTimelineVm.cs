using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class ActivityTimelineVm
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }
}
