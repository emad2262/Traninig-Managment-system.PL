using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICompanyRepo : IRepo<Company>

    {
        Task<DateTime?> GetCompanyExpirationDateAsync(int companyId);
      

        // جلب أفضل الموظفين كـ Entity
        Task<List<Employee>> GetTopPerformersAsync(int companyId, int take);
    }
}
