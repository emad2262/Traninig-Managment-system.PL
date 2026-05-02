namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class LessonsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeWorkspaceService _employeeWorkspaceService;

        public LessonsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeWorkspaceService employeeWorkspaceService)
        {
            _userManager = userManager;
            _employeeWorkspaceService = employeeWorkspaceService;
        }

        [HttpGet]
        public async Task<IActionResult> Watch(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _employeeWorkspaceService.GetLessonAsync(user.Id, lessonId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _employeeWorkspaceService.MarkLessonCompletedAsync(user.Id, lessonId);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (!result.IsSuccess)
            {
                return RedirectToAction(nameof(Watch), new { lessonId });
            }

            if (result.Data == null)
            {
                TempData["ErrorMessage"] = "The lesson completion result could not be loaded.";
                return RedirectToAction(nameof(Watch), new { lessonId });
            }

            if (result.Data.CertificateAvailable)
            {
                return RedirectToAction("Certificate", "EmployeeCourses", new
                {
                    area = "Employee",
                    courseId = result.Data.CourseId
                });
            }

            if (result.Data.NextLessonId.HasValue)
            {
                return RedirectToAction(nameof(Watch), new { lessonId = result.Data.NextLessonId.Value });
            }

            return RedirectToAction("Details", "EmployeeCourses", new
            {
                area = "Employee",
                courseId = result.Data.CourseId
            });
        }
    }
}
