using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeRepo : Repo<Employee>, IEmployeeRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public EmployeeRepo(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
