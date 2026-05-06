using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CompanySubscriptionService : ICompanySubscriptionService
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly ICourseRepo _courseRepo;

        public CompanySubscriptionService(
            ICompanyRepo companyRepo,
            IEmployeeRepo employeeRepo,
            ICourseRepo courseRepo)
        {
            _companyRepo = companyRepo;
            _employeeRepo = employeeRepo;
            _courseRepo = courseRepo;
        }

        public async Task<ServiceResult<bool>> EnsureActiveAsync(int companyId)
        {
            var company = await _companyRepo.GetOneAsync(c => c.Id == companyId, c => c.Plan);
            if (company == null)
            {
                return Fail("Company account was not found.");
            }

            if (!company.IsActive)
            {
                return Fail("Your company account is currently inactive. Please contact platform support.");
            }

            if (company.SubscriptionEnd.Date < DateTime.UtcNow.Date)
            {
                return Fail("Your subscription has expired. Please renew before making changes.");
            }

            if (company.Plan == null || !company.Plan.IsActive)
            {
                return Fail("Your current plan is not active. Please choose an active plan.");
            }

            return Ok();
        }

        public async Task<ServiceResult<bool>> EnsureCanAddEmployeeAsync(int companyId)
        {
            var active = await EnsureActiveAsync(companyId);
            if (!active.IsSuccess)
            {
                return active;
            }

            var company = await _companyRepo.GetOneAsync(c => c.Id == companyId, c => c.Plan);
            if (company?.Plan == null)
            {
                return Fail("Company plan could not be loaded.");
            }

            var activeEmployees = await _employeeRepo.CountAsync(e => e.CompanyId == companyId && e.IsActive);
            if (activeEmployees >= company.Plan.MaxEmployees)
            {
                return Fail($"Your plan allows up to {company.Plan.MaxEmployees} active employees. Upgrade your plan before adding more.");
            }

            return Ok();
        }

        public async Task<ServiceResult<bool>> EnsureCanCreateCourseAsync(int companyId)
        {
            var active = await EnsureActiveAsync(companyId);
            if (!active.IsSuccess)
            {
                return active;
            }

            var company = await _companyRepo.GetOneAsync(c => c.Id == companyId, c => c.Plan);
            if (company?.Plan == null)
            {
                return Fail("Company plan could not be loaded.");
            }

            var courses = await _courseRepo.CountAsync(c => c.Category.CompanyId == companyId);
            if (courses >= company.Plan.MaxCourses)
            {
                return Fail($"Your plan allows up to {company.Plan.MaxCourses} courses. Upgrade your plan before creating more.");
            }

            return Ok();
        }

        private static ServiceResult<bool> Ok()
        {
            return new ServiceResult<bool> { IsSuccess = true, Data = true };
        }

        private static ServiceResult<bool> Fail(string message)
        {
            return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = message };
        }
    }
}
