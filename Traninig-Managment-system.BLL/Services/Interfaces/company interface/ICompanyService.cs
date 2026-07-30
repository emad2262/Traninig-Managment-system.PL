using Traninig_Managment_system.BLL.Dtos.Company;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanyService
    {
        // ─── Read ──────────────────────────────────────────────
        Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);

        Task<CompanyDetailsDto?> GetCompanyByIdAsync(int companyId, CancellationToken cancellationToken = default);

        // ─── Create ────────────────────────────────────────────
        Task<int> CreateCompanyAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default);

        // ─── Update ────────────────────────────────────────────
        Task UpdateCompanyAsync(UpdateCompanyDto dto, CancellationToken cancellationToken = default);

        Task<bool> ToggleActiveAsync(int companyId, CancellationToken cancellationToken = default);

        // ─── Delete ────────────────────────────────────────────
        Task<bool> DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default);
    }
}
