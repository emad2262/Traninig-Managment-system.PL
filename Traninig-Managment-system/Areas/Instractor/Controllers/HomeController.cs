using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    public class HomeController : Controller
    {
        private readonly IInstructorWorkspaceService _workspaceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IInstructorWorkspaceService workspaceService, UserManager<ApplicationUser> userManager)
        {
            _workspaceService = workspaceService;
            _userManager = userManager;
        }

        public async Task<IActionResult> DashBoard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var dashboardVm = await _workspaceService.GetDashboardAsync(user.Id);
            if (dashboardVm == null) return NotFound("Instructor profile was not found.");

            return View(dashboardVm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var course = await _workspaceService.GetCourseDetailsAsync(id, user.Id);
            if (course == null) return NotFound();

            return View(course);
        }
    }
}
