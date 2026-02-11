using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CourseDetailsVm
    {
        public int Id { get; set; }
        public int DurationInHours { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        // قائمة الدروس
        public List<LessonDisplayVm> Lessons { get; set; } = new List<LessonDisplayVm>();
    }
}
