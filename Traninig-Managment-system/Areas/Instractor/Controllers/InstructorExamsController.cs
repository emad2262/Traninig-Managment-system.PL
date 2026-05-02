using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    public class InstructorExamsController : Controller
    {
        private readonly IInstructorWorkspaceService _workspaceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorExamsController(IInstructorWorkspaceService workspaceService, UserManager<ApplicationUser> userManager)
        {
            _workspaceService = workspaceService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId, int? chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _workspaceService.BuildExamCreateModelAsync(courseId, user.Id, chapterId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorExamFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            EnsureQuestionRows(model);
            if (!ModelState.IsValid)
            {
                await RebuildExamOptionsAsync(model, user.Id);
                return View(model);
            }

            var result = await _workspaceService.CreateExamAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await RebuildExamOptionsAsync(model, user.Id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _workspaceService.GetExamForEditAsync(id, user.Id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstructorExamFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            EnsureQuestionRows(model);
            if (!ModelState.IsValid)
            {
                await RebuildExamOptionsAsync(model, user.Id);
                return View(model);
            }

            var result = await _workspaceService.UpdateExamAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await RebuildExamOptionsAsync(model, user.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _workspaceService.ToggleExamPublishAsync(id, user.Id);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _workspaceService.DeleteExamAsync(id, user.Id);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }

        private static void EnsureQuestionRows(InstructorExamFormVm model)
        {
            model.Questions ??= new List<InstructorExamQuestionFormVm>();
            while (model.Questions.Count < 3)
            {
                model.Questions.Add(new InstructorExamQuestionFormVm());
            }
        }

        private async Task RebuildExamOptionsAsync(InstructorExamFormVm model, string userId)
        {
            var shell = await _workspaceService.BuildExamCreateModelAsync(model.CourseId, userId, model.ChapterId);
            model.ChapterOptions = shell?.ChapterOptions ?? new List<InstructorChapterOptionVm>();
            model.ChapterTitle = shell?.ChapterTitle;
        }
    }
}
