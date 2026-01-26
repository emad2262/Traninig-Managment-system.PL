
namespace Traninig_Managment_system.DAL.Repo
{
    public class AdminCompanyRepo : Repo<AdminPlatform>,IAdminPlatformRepo
    {
        public AdminCompanyRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
