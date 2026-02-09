using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CategoryVm
    {
        public int Id{ get; set; }
        public string CategoryName { get; set; }
        public ICollection<CourseVm> Courses{ get; set; }=new List<CourseVm>();
    }
}
