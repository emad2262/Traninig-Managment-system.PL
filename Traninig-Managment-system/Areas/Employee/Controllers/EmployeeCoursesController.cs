namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class EmployeeCoursesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeWorkspaceService _employeeWorkspaceService;

        public EmployeeCoursesController(
            UserManager<ApplicationUser> userManager,
            IEmployeeWorkspaceService employeeWorkspaceService)
        {
            _userManager = userManager;
            _employeeWorkspaceService = employeeWorkspaceService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard", "Home", new { area = "Employee" });
        }

        public async Task<IActionResult> Details(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _employeeWorkspaceService.GetCourseDetailsAsync(user.Id, courseId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        public async Task<IActionResult> Certificate(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _employeeWorkspaceService.GetCertificateAsync(user.Id, courseId);
            if (vm == null) return NotFound();

            return View(vm);
        }
    }
}
