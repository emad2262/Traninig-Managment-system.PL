using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class LessonVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public string? PdfUrl { get; set; }
        public int Order { get; set; }
        public int Duration { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsLocked { get; set; }  
    }
}
