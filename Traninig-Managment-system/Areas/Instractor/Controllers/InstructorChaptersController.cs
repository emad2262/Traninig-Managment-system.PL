using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    public class InstructorChaptersController : Controller
    {
        private readonly IInstructorWorkspaceService _workspaceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorChaptersController(
            IInstructorWorkspaceService workspaceService,
            UserManager<ApplicationUser> userManager)
        {
            _workspaceService = workspaceService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _workspaceService.BuildChapterCreateModelAsync(courseId, user.Id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorChapterFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _workspaceService.CreateChapterAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _workspaceService.GetChapterForEditAsync(id, user.Id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstructorChapterFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _workspaceService.UpdateChapterAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _workspaceService.DeleteChapterAsync(id, user.Id);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }
    }
}
