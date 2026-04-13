using Microsoft.AspNetCore.Identity;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyDashboardService _companyDashboardService;

        public HomeController(UserManager<ApplicationUser> userManager,ICompanyDashboardService companyDashboardService)
        {
            _userManager = userManager;
           _companyDashboardService = companyDashboardService;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // 1. نتأكد الأول إن اليوزر مش بـ null وإن عنده CompanyId
            if (currentUser == null || currentUser.CompanyId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // 2. هنا نقدر نستخدم .Value وإحنا مطمنين والتحذير هيختفي
            var dashboardVm = await _companyDashboardService.GetDashboardDataAsync(currentUser.CompanyId.Value);
            return View(dashboardVm);
        }
    }
}
