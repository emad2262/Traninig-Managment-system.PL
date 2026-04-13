

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeCourseRepo : Repo<EmployeeCourse>, IEmployeeCourseRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeCourseRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        
    }
}
