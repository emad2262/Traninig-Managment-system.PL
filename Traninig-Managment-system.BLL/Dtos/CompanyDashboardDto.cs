using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CompanyDashboardDto
    {
        public int EmployeeCount { get; set; }

        public int CourseCount { get; set; }

        public int CategoryCount { get; set; }

        public int InstructorCount { get; set; }

        public DateTime? SubscriptionEndDate { get; set; }
    }
}
