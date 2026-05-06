namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class ExamsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeExamService _examService;

        public ExamsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeExamService examService)
        {
            _userManager = userManager;
            _examService = examService;
        }

        [HttpGet]
        public async Task<IActionResult> Take(int examId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _examService.GetExamAsync(user.Id, examId);
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
                var retryVm = await _examService.GetExamAsync(user.Id, model.ExamId);
                if (retryVm == null) return NotFound();
                return View("Take", retryVm);
            }

            var result = await _examService.SubmitExamAsync(user.Id, model);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Take), new { examId = model.ExamId });
            }

            return View("Result", result.Data);
        }
    }
}
