using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeCourseRepo : Repo<EmployeeCourse>, IEmployeeCourseRepo
    {
        public EmployeeCourseRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
