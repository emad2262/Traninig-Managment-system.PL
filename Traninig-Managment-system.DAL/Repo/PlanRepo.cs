using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class PlanRepo : Repo<Plan>, IPlanRepo
    {
        public PlanRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
