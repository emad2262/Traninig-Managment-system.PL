namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeDashboardService _dashboardService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IEmployeeDashboardService dashboardService)
        {
            _userManager = userManager;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _dashboardService.GetDashboardAsync(user.Id);
            if (vm == null) return NotFound();

            return View(vm);
        }
    }
}
