using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class EditInstructorVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public bool Isactive { get; set; }

        public string Specialization { get; set; } = string.Empty;
    }
}
