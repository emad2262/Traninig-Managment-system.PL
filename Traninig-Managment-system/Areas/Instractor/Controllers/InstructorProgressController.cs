using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    public class InstructorProgressController : Controller
    {
        private readonly IInstructorWorkspaceService _workspaceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorProgressController(IInstructorWorkspaceService workspaceService, UserManager<ApplicationUser> userManager)
        {
            _workspaceService = workspaceService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var progress = await _workspaceService.GetEmployeeProgressAsync(user.Id, courseId);
            ViewBag.CourseId = courseId;
            return View(progress);
        }

        public async Task<IActionResult> Details(int employeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var employee = await _workspaceService.GetEmployeeDetailsAsync(employeeId, user.Id);
            if (employee == null) return NotFound();

            return View(employee);
        }
    }
}
