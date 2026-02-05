using Traninig_Managment_system.BLL.ModelVm;
using Traninig_Managment_system.DAL.Data;

public class StatisticsManager
{
    private readonly ApplicationDbContext _context;

    public StatisticsManager(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatisticsVM> GetDashboardStatisticsAsync()
    {
        var vm = new DashboardStatisticsVM();

        // ========== 1. الشركات الجديدة شهرياً (آخر 6 شهور) ==========
        var sixMonthsAgo = DateTime.Now.AddMonths(-6);

        // جيب الداتا الأول من الـ Database
        var companies = _context.companies
            .Where(c => c.SubscriptionStart >= sixMonthsAgo)
            .Select(c => new { c.SubscriptionStart })
            .ToList();

        // بعدين اعمل GroupBy في الـ Memory
        var companiesPerMonth = companies
            .GroupBy(c => new { c.SubscriptionStart.Year, c.SubscriptionStart.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        // تعبئة الشهور
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            var monthName = date.ToString("MMM yyyy");
            vm.Months.Add(monthName);

            var count = companiesPerMonth
                .FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month)?.Count ?? 0;
            vm.NewCompaniesPerMonth.Add(count);
        }

        // ========== 2. حساب نسبة النمو/الهبوط ==========
        var lastMonth = vm.NewCompaniesPerMonth.LastOrDefault();
        var previousMonth = vm.NewCompaniesPerMonth.Count > 1
            ? vm.NewCompaniesPerMonth[^2]
            : 0;

        if (previousMonth > 0)
        {
            vm.GrowthRate = ((decimal)(lastMonth - previousMonth) / previousMonth) * 100;
            vm.IsGrowing = vm.GrowthRate >= 0;
        }
        else if (lastMonth > 0)
        {
            vm.GrowthRate = 100;
            vm.IsGrowing = true;
        }

        // ========== 3. إحصائيات عامة ==========
        vm.TotalCompanies =_context.companies.Count();
        vm.ActiveCompanies =  _context.companies.Count(c => c.IsActive);
        vm.InactiveCompanies =  _context.companies.Count(c => !c.IsActive);

        // ========== 4. توزيع الشركات (Active vs Inactive) ==========
        vm.StatusLabels = new List<string> { "Active", "Inactive" };
        vm.StatusCounts = new List<int> { vm.ActiveCompanies, vm.InactiveCompanies };

        return vm;
    }
}