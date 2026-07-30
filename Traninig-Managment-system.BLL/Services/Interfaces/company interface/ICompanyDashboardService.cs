

using Traninig_Managment_system.BLL.Dtos;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanyDashboardService
    {
        Task<CompanyDashboardDto> GetDashboardAsync(
       int companyId
       );
    }
}
