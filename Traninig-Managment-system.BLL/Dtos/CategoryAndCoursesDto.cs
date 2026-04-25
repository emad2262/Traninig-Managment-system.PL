using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CategoryAndCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<CourseDto> Courses { get; set; } = new List<CourseDto>();
    }
}
