namespace Traninig_Managment_system.BLL.Services.classes
{
    public class DashboardService : IDashboardService
    {
        private readonly ICompanyRepo _companyRepo;

        public DashboardService(ICompanyRepo companyRepo)
        {
            _companyRepo = companyRepo;
        }
        public async Task<IEnumerable<Company>> GetAllCompaniesWithDetailsAsync()
        {
            var companies = await _companyRepo.GetAllAsync(
                null,
                c => c.Plan,
                c => c.Employees,
                companies=>companies.Courses
            );
            return companies;
        }

    }
}
