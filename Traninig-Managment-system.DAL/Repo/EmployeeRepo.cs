

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeRepo : Repo<Employee>, IEmployeeRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
