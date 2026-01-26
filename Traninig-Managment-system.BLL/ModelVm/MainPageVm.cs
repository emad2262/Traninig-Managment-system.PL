using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class MainPageVm
    {
        public IEnumerable<Plan> Plans { get; set; }
        public IEnumerable<Company> Companies { get; set; }
    }
}
