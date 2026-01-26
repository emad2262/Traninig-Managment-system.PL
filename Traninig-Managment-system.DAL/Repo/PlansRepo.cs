


namespace Traninig_Managment_system.DAL.Repo
{
    public class PlansRepo : Repo<Plan>,IplanRepo
    {
        public PlansRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
