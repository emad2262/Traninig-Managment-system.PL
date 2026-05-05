namespace Traninig_Managment_system.DAL.Repo
{
    public class CompanyNotificationRepo : Repo<CompanyNotification>, ICompanyNotificationRepo
    {
        public CompanyNotificationRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
