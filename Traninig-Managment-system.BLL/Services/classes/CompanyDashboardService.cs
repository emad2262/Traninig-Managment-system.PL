using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class CompanyDashboardService : ICompanyDashboardService
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly ICourseRepo _courseRepo;

        public CompanyDashboardService(ICompanyRepo companyRepo,ICourseRepo courseRepo)
        {
           _companyRepo = companyRepo;
           _courseRepo = courseRepo;
        }

        public async Task<CompanyOverviewVm> GetDashboardDataAsync(int companyId)
        {
            var vm = new CompanyOverviewVm();

            // 1. استدعاء الداتا الأساسية من الـ Repo
            vm.ExpirationDate = await _companyRepo.GetCompanyExpirationDateAsync(companyId) ?? DateTime.MinValue;
            vm.TotalEmployees = await _companyRepo.CountAsync(e => e.Employees == e.Employees && e.IsActive);
            vm.TotalCourses = await _courseRepo.CountAsync(e => e.Category.CompanyId == companyId && e.IsPublished);
            vm.CompletionRate = 68; // (سيتم برمجتها لاحقاً عند وجود جدول الدروس)

            // 3. معالجة وتغليف أفضل الموظفين (Mapping)
            var topEmployees = await _companyRepo.GetTopPerformersAsync(companyId, 3);
            int rank = 1;

            foreach (var emp in topEmployees)
            {
                vm.TopPerformers.Add(new TopEmployeeVm
                {
                    Rank = rank++,
                    EmployeeName = emp.Name,
                    JobTitle = emp.JobTitle ?? "Employee",
                    Points = emp.Points
                });
            }

            return vm;
        }
    }
}