

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class MainPageServices : IMainPage
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly IplanRepo _iplanRepo;

        public MainPageServices(ICompanyRepo companyRepo, IplanRepo iplanRepo)
        {
            _companyRepo = companyRepo;
            _iplanRepo = iplanRepo;
        }
        public async Task<IEnumerable<Company>> GetCompaniesAsync()
        {
           return await _companyRepo.GetAllAsync();
        }

        public async Task<IEnumerable<Plan>> GetPlansAsync()
        {
            return await _iplanRepo.GetAllAsync();
        }
    }
}
