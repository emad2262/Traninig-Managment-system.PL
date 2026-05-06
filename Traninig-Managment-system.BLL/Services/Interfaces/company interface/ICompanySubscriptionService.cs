namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanySubscriptionService
    {
        Task<ServiceResult<bool>> EnsureActiveAsync(int companyId);
        Task<ServiceResult<bool>> EnsureCanAddEmployeeAsync(int companyId);
        Task<ServiceResult<bool>> EnsureCanCreateCourseAsync(int companyId);
    }
}
