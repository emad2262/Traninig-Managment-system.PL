using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CourseLessonsVm
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<LessonDisplayVm> Lessons { get; set; }
    }
}
