using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class EmployeeIndexVm
    {
        // 1. دي اللستة اللي هتلوب عليها في الجدول
        public IEnumerable<ListEmployeeVm> Employees { get; set; } = new List<ListEmployeeVm>();

        // 2. دي كلمة البحث عشان نحتفظ بيها في الشاشة
        public string Name { get; set; } = string.Empty;

        // 3. دي بيانات الـ Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
