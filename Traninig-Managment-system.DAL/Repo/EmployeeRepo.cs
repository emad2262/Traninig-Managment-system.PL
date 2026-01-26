

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeRepo : Repo<Employee>
    {
        public EmployeeRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
