using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class LessonDetailsVm
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string? VideoUrl { get; set; }
        public string? PdfUrl { get; set; }

        public bool IsCompleted { get; set; }
    }

}
