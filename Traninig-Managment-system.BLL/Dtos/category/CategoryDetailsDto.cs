using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.BLL.Dtos.Course;
using Traninig_Managment_system.BLL.Dtos.Lessons;

namespace Traninig_Managment_system.BLL.Dtos.category
{
    public class CategoryDetailsDto
    {
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalCourse { get; set; }
        public int PublishedCourses { get; set; }
        public List<ListCourseDto> CourseListDtos { get; set; } = new();
    }
}
