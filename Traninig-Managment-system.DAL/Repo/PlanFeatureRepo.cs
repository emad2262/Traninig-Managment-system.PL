namespace Traninig_Managment_system.DAL.Repo
{
    public class PlanFeatureRepo : Repo<PlanFeature>, IPlanFeatureRepo
    {
        public PlanFeatureRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
