namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = SD.Employee)]
    public class LessonsController : Controller
    {
        private const double AutoCompletionThreshold = 90.0;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeWorkspaceService _employeeWorkspaceService;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IEmployeeLessonRepo _employeeLessonRepo;
        private readonly ILessonRepo _lessonRepo;

        public LessonsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeWorkspaceService employeeWorkspaceService,
            IEmployeeRepo employeeRepo,
            IEmployeeLessonRepo employeeLessonRepo,
            ILessonRepo lessonRepo)
        {
            _userManager = userManager;
            _employeeWorkspaceService = employeeWorkspaceService;
            _employeeRepo = employeeRepo;
            _employeeLessonRepo = employeeLessonRepo;
            _lessonRepo = lessonRepo;
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

        // ===========================================================
        // Auto-progress JSON endpoints
        // The Watch view's video player calls these. Watching the
        // video is what marks a lesson done — no manual button needed.
        // ===========================================================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveProgress([FromBody] ProgressDto dto)
        {
            if (dto == null) return BadRequest(new { ok = false });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var employee = await _employeeRepo.GetOneAsync(e => e.UserId == user.Id && e.IsActive);
            if (employee == null) return NotFound(new { ok = false });

            var lesson = await _lessonRepo.GetOneAsync(l => l.Id == dto.LessonId);
            if (lesson == null) return NotFound(new { ok = false });

            var watched = Math.Clamp(dto.WatchedPercentage, 0, 100);
            var lastSecond = Math.Max(dto.LastWatchedSecond, 0);

            var record = await _employeeLessonRepo.GetOneAsync(
                el => el.EmployeeId == employee.Id && el.LessonId == dto.LessonId);

            bool wasCompleted = record?.IsCompleted ?? false;

            if (record == null)
            {
                record = new EmployeeLesson
                {
                    EmployeeId = employee.Id,
                    LessonId = dto.LessonId,
                    WatchedPercentage = watched,
                    LastWatchedSecond = lastSecond,
                    StartedAt = DateTime.UtcNow,
                    IsCompleted = false
                };
                await _employeeLessonRepo.CreateAsync(record);
            }
            else
            {
                if (watched > record.WatchedPercentage) record.WatchedPercentage = watched;
                if (lastSecond > record.LastWatchedSecond) record.LastWatchedSecond = lastSecond;
                if (!wasCompleted) await _employeeLessonRepo.UpdateAsync(record);
            }

            // Crossed the auto-completion threshold? Hand off to the
            // workspace service so points/badges/course state stay in sync.
            if (!wasCompleted && record.WatchedPercentage >= AutoCompletionThreshold)
            {
                var completion = await _employeeWorkspaceService.MarkLessonCompletedAsync(user.Id, dto.LessonId);
                if (completion.IsSuccess && completion.Data != null)
                {
                    return Json(new
                    {
                        ok = true,
                        watchedPercentage = record.WatchedPercentage,
                        completed = true,
                        justCompleted = true,
                        nextLessonId = completion.Data.NextLessonId,
                        certificateAvailable = completion.Data.CertificateAvailable,
                        courseCompleted = completion.Data.CourseCompleted,
                        courseId = completion.Data.CourseId,
                        message = completion.Message
                    });
                }
            }

            return Json(new
            {
                ok = true,
                watchedPercentage = record.WatchedPercentage,
                completed = record.IsCompleted,
                justCompleted = false
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AutoComplete([FromBody] AutoCompleteDto dto)
        {
            if (dto == null) return BadRequest(new { ok = false });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _employeeWorkspaceService.MarkLessonCompletedAsync(user.Id, dto.LessonId);
            if (!result.IsSuccess || result.Data == null)
            {
                return Json(new { ok = false, message = result.Message });
            }

            return Json(new
            {
                ok = true,
                completed = true,
                justCompleted = true,
                nextLessonId = result.Data.NextLessonId,
                certificateAvailable = result.Data.CertificateAvailable,
                courseCompleted = result.Data.CourseCompleted,
                courseId = result.Data.CourseId,
                message = result.Message
            });
        }

        public class ProgressDto
        {
            public int LessonId { get; set; }
            public double WatchedPercentage { get; set; }
            public double LastWatchedSecond { get; set; }
        }

        public class AutoCompleteDto
        {
            public int LessonId { get; set; }
        }
    }
}
