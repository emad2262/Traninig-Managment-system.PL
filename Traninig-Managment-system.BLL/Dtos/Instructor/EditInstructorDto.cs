using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Instructor
{
    public class EditInstructorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }

    }
}
