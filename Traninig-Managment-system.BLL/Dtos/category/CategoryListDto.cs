using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.category
{
    public class CategoryListDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int TotalCourses { get; set; }

    }
    
}
