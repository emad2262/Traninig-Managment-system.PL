using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class LessonVm
    {
        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public IFormFile File { get; set; }

        public int Order { get; set; }
    }
}
