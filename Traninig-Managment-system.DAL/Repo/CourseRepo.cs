using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CourseRepo : Repo<Course>, ICourseRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public CourseRepo(ApplicationDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
