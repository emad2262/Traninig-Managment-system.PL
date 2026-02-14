using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CourseLessons
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public string CategoryName { get; set; } = "";

        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercentage { get; set; }

        public List<LessonVm> Lessons { get; set; } = new();
    }
}
