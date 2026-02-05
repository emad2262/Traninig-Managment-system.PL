using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class EditEmployeeVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
    }
}
