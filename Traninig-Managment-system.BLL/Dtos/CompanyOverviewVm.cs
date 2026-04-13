using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CompanyOverviewVm
    {
        // 1. Subscription & Quota
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpirationDate { get; set; }


        // 2. KPIs
        public int TotalEmployees { get; set; }
        public int TotalCourses { get; set; } // الكورسات الـ Published
        public int CompletionRate { get; set; } // متوسط الإنجاز للشركة
        public int ActiveInstructors { get; set; }
        // 2. Data Lists
        public List<TopEmployeeVm> TopPerformers { get; set; } = new List<TopEmployeeVm>();
        public List<ActivityTimelineVm> RecentActivities { get; set; } = new List<ActivityTimelineVm>();
    }
}
