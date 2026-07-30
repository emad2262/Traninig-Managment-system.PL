using Traninig_Managment_system.BLL.Dtos.Company;
using Traninig_Managment_system.BLL.Services.Interfaces;
using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepo _companyRepo;

        public CompanyService(ICompanyRepo companyRepo)
        {
            _companyRepo = companyRepo;
        }

        // ─── Read ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<CompanyListDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
        {
            var companies = await _companyRepo.GetAllAsync(cancellationToken: cancellationToken);

            return companies.Select(c => new CompanyListDto
            {
                Id              = c.Id,
                Name            = c.Name,
                Email           = c.Email,
                Logo            = c.Logo,
                IsActive        = c.IsActive,
                SubscriptionEnd = c.SubscriptionEnd,
                PlanName        = c.Plan?.Name ?? string.Empty
            });
        }

        public async Task<CompanyDetailsDto?> GetCompanyByIdAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepo.GetOneAsync(
                c => c.Id == companyId,
                cancellationToken);

            if (company is null)
                return null;

            return new CompanyDetailsDto
            {
                Id                = company.Id,
                Name              = company.Name,
                Email             = company.Email,
                Logo              = company.Logo,
                IsActive          = company.IsActive,
                SubscriptionStart = company.SubscriptionStart,
                SubscriptionEnd   = company.SubscriptionEnd,

                PlanId            = company.PlanId,
                PlanName          = company.Plan?.Name ?? string.Empty,
                MaxEmployees      = company.Plan?.MaxEmployees ?? 0,
                MaxCourses        = company.Plan?.MaxCourses ?? 0,

                TotalEmployees    = company.Employees?.Count ?? 0,
                TotalInstructors  = company.Instructors?.Count ?? 0,
                TotalCourses      = company.CoursesCategories?.Sum(cat => cat.Courses?.Count ?? 0) ?? 0
            };
        }

        // ─── Create ────────────────────────────────────────────────────────────

        public async Task<int> CreateCompanyAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default)
        {
            var emailTaken = await _companyRepo.IsEmailTakenAsync(dto.Email, cancellationToken: cancellationToken);
            if (emailTaken)
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

            var company = new Company
            {
                Name              = dto.Name,
                Email             = dto.Email,
                Logo              = dto.Logo,
                PlanId            = dto.PlanId,
                SubscriptionStart = dto.SubscriptionStart,
                SubscriptionEnd   = dto.SubscriptionEnd,
                IsActive          = true
            };

            await _companyRepo.CreateAsync(company);
            await _companyRepo.SaveChangesAsync(cancellationToken);

            return company.Id;
        }

        // ─── Update ────────────────────────────────────────────────────────────

        public async Task UpdateCompanyAsync(UpdateCompanyDto dto, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepo.GetOneAsync(
                c => c.Id == dto.Id,
                cancellationToken);

            if (company is null)
                throw new InvalidOperationException($"Company with Id {dto.Id} was not found.");

            var emailTaken = await _companyRepo.IsEmailTakenAsync(dto.Email, excludeId: dto.Id, cancellationToken: cancellationToken);
            if (emailTaken)
                throw new InvalidOperationException($"Email '{dto.Email}' is already used by another company.");

            company.Name              = dto.Name;
            company.Email             = dto.Email;
            company.Logo              = dto.Logo;
            company.IsActive          = dto.IsActive;
            company.PlanId            = dto.PlanId;
            company.SubscriptionStart = dto.SubscriptionStart;
            company.SubscriptionEnd   = dto.SubscriptionEnd;

            await _companyRepo.Update(company);
            await _companyRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ToggleActiveAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepo.GetOneAsync(
                c => c.Id == companyId,
                cancellationToken);

            if (company is null)
                return false;

            company.IsActive = !company.IsActive;

            await _companyRepo.Update(company);
            await _companyRepo.SaveChangesAsync(cancellationToken);

            return company.IsActive;
        }

        // ─── Delete ────────────────────────────────────────────────────────────

        public async Task<bool> DeleteCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepo.GetOneAsync(
                c => c.Id == companyId,
                cancellationToken);

            if (company is null)
                return false;

            await _companyRepo.Delete(company);
            await _companyRepo.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
