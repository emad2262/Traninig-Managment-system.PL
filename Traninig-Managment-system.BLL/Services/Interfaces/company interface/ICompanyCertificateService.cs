namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanyCertificateService
    {
        Task<IReadOnlyList<CompanyCertificateListItemVm>> GetCertificatesAsync(int companyId, CertificateStatus? status = null);
        Task<CompanyCertificateDetailsVm?> GetCertificateDetailsAsync(int companyId, int certificateId);
        Task<ServiceResult<bool>> IssueCertificateAsync(int companyId, CompanyCertificateIssueVm model, string issuedByUserId, string? certificateUrl);
    }
}
