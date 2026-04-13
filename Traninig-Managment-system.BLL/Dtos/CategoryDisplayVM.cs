using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CategoryDisplayVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        // ميزة إضافية للـ UI: عرض عدد الكورسات جوه كل قسم
        public int TotalCourses { get; set; }
    }
}
