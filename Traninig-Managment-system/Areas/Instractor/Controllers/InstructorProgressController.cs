using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    [Authorize(Roles = SD.Instructor)]
    public class InstructorProgressController : Controller
    {
        private readonly IInstructorProgressService _progressService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorProgressController(IInstructorProgressService progressService, UserManager<ApplicationUser> userManager)
        {
            _progressService = progressService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var progress = await _progressService.GetEmployeeProgressAsync(user.Id, courseId);
            ViewBag.CourseId = courseId;
            return View(progress);
        }

        public async Task<IActionResult> Details(int employeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var employee = await _progressService.GetEmployeeDetailsAsync(employeeId, user.Id);
            if (employee == null) return NotFound();

            return View(employee);
        }
    }
}
