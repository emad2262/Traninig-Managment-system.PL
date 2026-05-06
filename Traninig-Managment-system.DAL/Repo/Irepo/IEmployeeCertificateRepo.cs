using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IEmployeeCertificateRepo : IRepo<EmployeeCertificate>
    {
        Task<EmployeeCertificate?> GetByEmployeeCourseAsync(int employeeId, int courseId);
        Task<EmployeeCertificate?> GetForUpdateByEmployeeCourseAsync(int employeeId, int courseId);
        Task<EmployeeCertificate?> GetForUpdateAsync(int companyId, int certificateId);
        Task<EmployeeCertificate?> GetIssuedForEmployeeCourseAsync(int employeeId, int courseId);
        Task<EmployeeCertificate?> GetCompanyCertificateAsync(int companyId, int certificateId);
        Task<List<EmployeeCertificate>> GetCompanyCertificatesAsync(int companyId, CertificateStatus? status = null);
        Task<List<EmployeeCertificate>> GetRecentCompanyCertificatesAsync(int companyId, int take);
        Task<int> CountPendingAsync(int companyId);
        Task<bool> DeleteByEmployeeCourseAsync(int employeeId, int courseId);
        Task<bool> DeleteByEmployeeAsync(int employeeId);
        Task<bool> DeleteByCourseAsync(int courseId);
    }
}
