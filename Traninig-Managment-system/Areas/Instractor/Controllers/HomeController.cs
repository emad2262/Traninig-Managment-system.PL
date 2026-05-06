using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    [Authorize(Roles = SD.Instructor)]
    public class HomeController : Controller
    {
        private readonly IInstructorDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IInstructorDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> DashBoard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var dashboardVm = await _dashboardService.GetDashboardAsync(user.Id);
            if (dashboardVm == null) return NotFound("Instructor profile was not found.");

            return View(dashboardVm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var course = await _dashboardService.GetCourseDetailsAsync(id, user.Id);
            if (course == null) return NotFound();

            return View(course);
        }
    }
}
