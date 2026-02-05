using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CreaeInstructorVm
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Specialization { get; set; }
        public bool IsActive { get; set; }
    }
}
