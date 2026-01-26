
namespace Traninig_Managment_system.DAL.Repo
{
    public class FeatureRepo : Repo<Feature>
    {
        public FeatureRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
