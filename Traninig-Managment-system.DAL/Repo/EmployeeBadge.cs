using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeBadge : Repo<EmployeeBadge>, IEmpolyeeBadge
    {
        public EmployeeBadge(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
