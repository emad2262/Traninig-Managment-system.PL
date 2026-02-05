using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CreateEmployeeVm
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        
    }
}
