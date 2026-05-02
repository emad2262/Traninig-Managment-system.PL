namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeWorkspaceService _employeeWorkspaceService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IEmployeeWorkspaceService employeeWorkspaceService)
        {
            _userManager = userManager;
            _employeeWorkspaceService = employeeWorkspaceService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _employeeWorkspaceService.GetDashboardAsync(user.Id);
            if (vm == null) return NotFound();

            return View(vm);
        }
    }
}
