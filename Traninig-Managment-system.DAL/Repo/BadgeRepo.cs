using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class BadgeRepo : Repo<Badge>, IBadgeRepo
    {
        public BadgeRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
