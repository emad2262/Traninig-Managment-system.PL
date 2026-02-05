
namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICategoryCourseServices
    {
        Task<HomeCompanyVm> GetCompanyHomeDataAsync(int CompanyId);
    }
}
