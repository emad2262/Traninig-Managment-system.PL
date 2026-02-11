using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class EditLessonVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
