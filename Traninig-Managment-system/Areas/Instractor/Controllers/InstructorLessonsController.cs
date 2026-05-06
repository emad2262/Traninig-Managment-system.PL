using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    [Authorize(Roles = SD.Instructor)]
    public class InstructorLessonsController : Controller
    {
        private const string LessonUploadFolder = "uploads/lessons";

        private readonly IInstructorContentService _contentService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorLessonsController(
            IInstructorContentService contentService,
            IFileService fileService,
            UserManager<ApplicationUser> userManager)
        {
            _contentService = contentService;
            _fileService = fileService;
            _userManager = userManager;
        }

        public IActionResult LessonDisplay(int courseId)
        {
            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateLessons(int courseId, int? chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var model = await _contentService.BuildLessonCreateModelAsync(courseId, user.Id, chapterId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLessons(InstructorLessonFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                await RebuildLessonOptionsAsync(model, user.Id);
                return View(model);
            }

            if (model.File != null && model.File.Length > 0)
            {
                model.ExistingContentUrl = await _fileService.UploadFileAsync(model.File, LessonUploadFolder);
            }

            var result = await _contentService.CreateLessonAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            if (!string.IsNullOrWhiteSpace(model.ExistingContentUrl))
            {
                _fileService.DeleteFile(model.ExistingContentUrl);
                model.ExistingContentUrl = null;
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await RebuildLessonOptionsAsync(model, user.Id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditLessons(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var lesson = await _contentService.GetLessonForEditAsync(lessonId, user.Id);
            if (lesson == null) return NotFound();

            return View(lesson);
        }

        [HttpPost]
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLessons(InstructorLessonFormVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                await RebuildLessonOptionsAsync(model, user.Id);
                return View(model);
            }

            model.ExistingContentUrl = await _fileService.UpdateFileAsync(
                model.File,
                model.ExistingContentUrl,
                LessonUploadFolder);

            var result = await _contentService.UpdateLessonAsync(model, user.Id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await RebuildLessonOptionsAsync(model, user.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int lessonId, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _contentService.DeleteLessonAsync(lessonId, user.Id);
            if (result.IsSuccess)
            {
                _fileService.DeleteFile(result.Data);
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }

        private async Task RebuildLessonOptionsAsync(InstructorLessonFormVm model, string userId)
        {
            var shell = await _contentService.BuildLessonCreateModelAsync(model.CourseId, userId, model.ChapterId);
            model.ChapterOptions = shell?.ChapterOptions ?? new List<InstructorChapterOptionVm>();
        }
    }
}
