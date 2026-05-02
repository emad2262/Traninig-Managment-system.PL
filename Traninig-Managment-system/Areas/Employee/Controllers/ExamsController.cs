namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class ExamsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeWorkspaceService _employeeWorkspaceService;

        public ExamsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeWorkspaceService employeeWorkspaceService)
        {
            _userManager = userManager;
            _employeeWorkspaceService = employeeWorkspaceService;
        }

        [HttpGet]
        public async Task<IActionResult> Take(int examId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _employeeWorkspaceService.GetExamAsync(user.Id, examId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(EmployeeExamSubmissionVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var retryVm = await _employeeWorkspaceService.GetExamAsync(user.Id, model.ExamId);
                if (retryVm == null) return NotFound();
                return View("Take", retryVm);
            }

            var result = await _employeeWorkspaceService.SubmitExamAsync(user.Id, model);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Take), new { examId = model.ExamId });
            }

            return View("Result", result.Data);
        }
    }
}
