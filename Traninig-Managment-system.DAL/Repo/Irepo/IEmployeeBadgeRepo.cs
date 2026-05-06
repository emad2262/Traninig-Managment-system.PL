using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IEmployeeBadgeRepo : IRepo<EmployeeBadge>
    {
        Task<List<EmployeeBadge>> GetRecentCompanyBadgesAsync(int companyId, int take);
    }
}
