using Microsoft.AspNetCore.Http;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    [Authorize(Roles = SD.Instructor)]
    public class InstructorLessonsController : Controller
    {
        private const string LessonUploadFolder = "uploads/lessons";
        private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".webm" };
        private static readonly string[] PdfExtensions = { ".pdf" };

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

            ValidateLessonUploads(model);
            if (!ModelState.IsValid)
            {
                await RebuildLessonOptionsAsync(model, user.Id);
                return View(model);
            }

            try
            {
                model.ExistingVideoUrl = await UploadLessonFileAsync(model.VideoFile);
                model.ExistingPdfUrl = await UploadLessonFileAsync(model.PdfFile);
                model.ExistingContentUrl = model.ExistingVideoUrl ?? model.ExistingPdfUrl;

                var result = await _contentService.CreateLessonAsync(model, user.Id);
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
                }

                DeleteLessonFiles(model.ExistingVideoUrl, model.ExistingPdfUrl);
                ClearUploadedLessonUrls(model);

                ModelState.AddModelError(string.Empty, result.Message);
            }
            catch (Exception ex)
            {
                DeleteLessonFiles(model.ExistingVideoUrl, model.ExistingPdfUrl);
                ClearUploadedLessonUrls(model);
                ModelState.AddModelError(string.Empty, ex.Message);
            }

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

            ValidateLessonUploads(model);
            if (!ModelState.IsValid)
            {
                await RebuildLessonOptionsAsync(model, user.Id);
                return View(model);
            }

            var oldVideoUrl = model.ExistingVideoUrl;
            var oldPdfUrl = model.ExistingPdfUrl;
            string? newVideoUrl = null;
            string? newPdfUrl = null;

            try
            {
                newVideoUrl = await UploadLessonFileAsync(model.VideoFile);
                newPdfUrl = await UploadLessonFileAsync(model.PdfFile);

                if (!string.IsNullOrWhiteSpace(newVideoUrl))
                {
                    model.ExistingVideoUrl = newVideoUrl;
                }

                if (!string.IsNullOrWhiteSpace(newPdfUrl))
                {
                    model.ExistingPdfUrl = newPdfUrl;
                }

                model.ExistingContentUrl = model.ExistingVideoUrl ?? model.ExistingPdfUrl;

                var result = await _contentService.UpdateLessonAsync(model, user.Id);
                if (result.IsSuccess)
                {
                    DeleteReplacedFile(newVideoUrl, oldVideoUrl);
                    DeleteReplacedFile(newPdfUrl, oldPdfUrl);
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Details", "Home", new { area = "Instractor", id = model.CourseId });
                }

                DeleteLessonFiles(newVideoUrl, newPdfUrl);
                model.ExistingVideoUrl = oldVideoUrl;
                model.ExistingPdfUrl = oldPdfUrl;
                model.ExistingContentUrl = model.ExistingVideoUrl ?? model.ExistingPdfUrl;
                ModelState.AddModelError(string.Empty, result.Message);
            }
            catch (Exception ex)
            {
                DeleteLessonFiles(newVideoUrl, newPdfUrl);
                model.ExistingVideoUrl = oldVideoUrl;
                model.ExistingPdfUrl = oldPdfUrl;
                model.ExistingContentUrl = model.ExistingVideoUrl ?? model.ExistingPdfUrl;
                ModelState.AddModelError(string.Empty, ex.Message);
            }

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
                DeleteLessonFiles(result.Data);
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }

        private async Task<string?> UploadLessonFileAsync(IFormFile? file)
        {
            return HasFile(file)
                ? await _fileService.UploadFileAsync(file, LessonUploadFolder)
                : null;
        }

        private void ValidateLessonUploads(InstructorLessonFormVm model)
        {
            if (HasFile(model.VideoFile) && !HasAllowedExtension(model.VideoFile, VideoExtensions))
            {
                ModelState.AddModelError(nameof(model.VideoFile), "Please upload a valid video file.");
            }

            if (HasFile(model.PdfFile) && !HasAllowedExtension(model.PdfFile, PdfExtensions))
            {
                ModelState.AddModelError(nameof(model.PdfFile), "Please upload a PDF file.");
            }
        }

        private static bool HasFile(IFormFile? file)
        {
            return file != null && file.Length > 0;
        }

        private static bool HasAllowedExtension(IFormFile? file, IEnumerable<string> allowedExtensions)
        {
            if (!HasFile(file))
            {
                return true;
            }

            var extension = Path.GetExtension(file!.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        private void DeleteLessonFiles(params string?[] urls)
        {
            foreach (var url in urls.Where(url => !string.IsNullOrWhiteSpace(url)))
            {
                _fileService.DeleteFile(url);
            }
        }

        private void DeleteLessonFiles(IEnumerable<string>? urls)
        {
            if (urls == null)
            {
                return;
            }

            foreach (var url in urls)
            {
                _fileService.DeleteFile(url);
            }
        }

        private void DeleteReplacedFile(string? newUrl, string? oldUrl)
        {
            if (!string.IsNullOrWhiteSpace(newUrl) &&
                !string.IsNullOrWhiteSpace(oldUrl) &&
                !string.Equals(newUrl, oldUrl, StringComparison.OrdinalIgnoreCase))
            {
                _fileService.DeleteFile(oldUrl);
            }
        }

        private static void ClearUploadedLessonUrls(InstructorLessonFormVm model)
        {
            model.ExistingContentUrl = null;
            model.ExistingVideoUrl = null;
            model.ExistingPdfUrl = null;
        }

        private async Task RebuildLessonOptionsAsync(InstructorLessonFormVm model, string userId)
        {
            var shell = await _contentService.BuildLessonCreateModelAsync(model.CourseId, userId, model.ChapterId);
            model.ChapterOptions = shell?.ChapterOptions ?? new List<InstructorChapterOptionVm>();
        }
    }
}
