namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IManagerAreaService
    {
        Task<ManagerDashboardVm> GetDashboardAsync();

        Task<List<ManagerPlanVm>> GetPlansAsync();
        Task<ManagerPlanVm?> GetPlanAsync(int id);
        Task<ServiceResult<int>> CreatePlanAsync(ManagerPlanVm model);
        Task<ServiceResult<bool>> UpdatePlanAsync(ManagerPlanVm model);
        Task<ServiceResult<bool>> DeletePlanAsync(int id);

        Task<List<ManagerCompanyVm>> GetCompaniesAsync(string? search = null);
        Task<ManagerCompanyDetailsVm?> GetCompanyDetailsAsync(int id);
        Task<ServiceResult<int>> SendNotificationAsync(CreateCompanyNotificationVm model);
        Task<ServiceResult<int>> SendRenewalReminderAsync(int companyId);
    }
}
