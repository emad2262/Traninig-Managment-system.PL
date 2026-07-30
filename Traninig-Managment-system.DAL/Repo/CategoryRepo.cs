using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class InstructorRepo : Repo<Instructor>, IInstructorRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public InstructorRepo(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
