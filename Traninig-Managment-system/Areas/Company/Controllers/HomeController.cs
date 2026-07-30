using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class HomeController : Controller
    {
        private readonly ICompanyDashboardService _companyDashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ICompanyDashboardService companyDashboardService,UserManager<ApplicationUser> userManager)
        {
            _companyDashboardService = companyDashboardService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            int companyId = user.CompanyId.Value;

            var dashboard = await _companyDashboardService.GetDashboardAsync(companyId);

            return View(dashboard);

        }
    }
}
