
namespace Traninig_Managment_system.DAL.Repo
{
    public class CompanyRepo : Repo<Company>, ICompanyRepo
    {
        public CompanyRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
