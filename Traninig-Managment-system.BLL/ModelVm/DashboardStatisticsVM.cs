using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class DashboardStatisticsVM
    {
        // بيانات الشركات شهرياً
        public List<string> Months { get; set; } = new();
        public List<int> NewCompaniesPerMonth { get; set; } = new();

        // نسبة النمو/الهبوط
        public decimal GrowthRate { get; set; }
        public bool IsGrowing { get; set; }

        // إحصائيات عامة
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int InactiveCompanies { get; set; }

        // توزيع الشركات (لو عندك Status أو Category)
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
    }
}
