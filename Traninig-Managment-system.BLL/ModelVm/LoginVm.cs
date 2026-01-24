using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class LoginVm
    {
        public string Email{ get; set; }
        public string Password{ get; set; }

        public bool RememberMe{ get; set; }

    }
}
