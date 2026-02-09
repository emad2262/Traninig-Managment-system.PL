using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CreateLessonVm
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }

        // السيرفس تستقبل المسار مش الملف
        public string ContentUrl { get; set; }
    }
}
