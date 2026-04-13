using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeBadgeRepo : Repo<EmployeeBadge>, IEmployeeBadgeRepo
    {
        public EmployeeBadgeRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
