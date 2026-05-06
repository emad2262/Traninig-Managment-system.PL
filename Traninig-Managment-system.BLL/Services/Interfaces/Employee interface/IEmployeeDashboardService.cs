namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeDashboardService
    {
        Task<EmployeeDashboardVm?> GetDashboardAsync(string userId);
        Task<EmployeeCourseDetailsVm?> GetCourseDetailsAsync(string userId, int courseId);
        Task<EmployeeCertificateVm?> GetCertificateAsync(string userId, int courseId);
    }
}
