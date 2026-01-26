using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    public class DashBoardController : Controller
    {
        private readonly IDashboardService _dashboard;

        public DashBoardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }
        public async Task<IActionResult> Index(int page = 1)
        {

            var companies = await _dashboard.GetAllCompaniesWithDetailsAsync();

            // ✅ احسب الإجماليات الأول (قبل الـ Pagination)
            int totalCompanies = companies.Count();
            int totalEmployees = companies.Sum(c => c.Employees?.Count ??0);
            int totalCourses = companies.Sum(c => c.Courses?.Count ?? 0);

            // pagination can be added here later
            int pageSize = 4;
            var totalpage = (int)Math.Ceiling((double)companies.Count() / pageSize);
            companies = companies.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;  // ⬅️ ضيف ده
            ////
            DashboardVm dashboard = new DashboardVm
            {
                TotalEmployees = totalEmployees,
                TotalCourses = totalCourses,
                ExpiringSoon = companies.Count(c =>
                    c.SubscriptionEnd <= DateTime.Now.AddDays(30)
        ),
                totalpage = totalpage,
                companyList = companies.ToList(),
            };
            return View(dashboard);

        }
    }
}
