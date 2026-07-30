using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Employee
{
    public class CreateEmployeDto
    {
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
