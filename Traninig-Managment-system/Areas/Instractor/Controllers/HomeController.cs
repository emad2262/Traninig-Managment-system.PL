using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{      
    [Area("Instractor")]
    public class HomeController : Controller
    {
        private readonly IDashBoardInstractorServices _instractorServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IDashBoardInstractorServices instractorServices,UserManager<ApplicationUser> userManager)
        {
            _instractorServices = instractorServices;
            _userManager = userManager;
        }

        public async Task<IActionResult> DashBoard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound("user is not here"); }

            var companyid = user.CompanyId.Value;

            // 2️⃣ ننده على السيرفس
            var dashboardVm = await _instractorServices
                .GetDashboardAsync(companyid,user.Id);
            return View(dashboardVm);
        }
    }
}
