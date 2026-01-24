using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CompanyRepo : Repo<Company>
    {
        public CompanyRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
