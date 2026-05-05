using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CourseController : Controller
    {
        private const string CourseLogoFolder = "course-logos";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly ICourseServices _courseService;
        private readonly IInstructorServices _instructorServices;

        public CourseController(
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            ICourseServices courseServices,
            IInstructorServices instructorServices)
        {
            _userManager = userManager;
            _fileService = fileService;
            _courseService = courseServices;
            _instructorServices = instructorServices;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var courses = categoryId.HasValue
                ? await _courseService.GetAllInCategoryAsync(companyId.Value, categoryId.Value)
                : await _courseService.GetCoursesAsync(companyId.Value);

            ViewBag.CategoryId = categoryId;
            ViewBag.IsCategoryView = categoryId.HasValue;
            return View(courses);
        }

        public async Task<IActionResult> Details(int id, int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            ViewBag.CategoryId = categoryId ?? course.CategoryId;
            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            await LoadInstructorsAsync(companyId.Value);
            return View(new CourseDto
            {
                CategoryId = categoryId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseDto model, IFormFile? logoFile)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                await LoadInstructorsAsync(companyId.Value, model.InstructorId);
                return View(model);
            }
            if (logoFile != null && logoFile.Length > 0)
            {
                model.Logo = await _fileService.UploadFileAsync(logoFile, CourseLogoFolder);
            }

            var result = await _courseService.CreateCourseAsync(model, companyId.Value);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
            }

            if (!string.IsNullOrEmpty(model.Logo))
            {
                _fileService.DeleteFile(model.Logo);
                model.Logo = null;
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await LoadInstructorsAsync(companyId.Value, model.InstructorId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            await LoadInstructorsAsync(companyId.Value, course.InstructorId);
            ViewBag.CategoryId = categoryId ?? course.CategoryId;
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseDto model, IFormFile? logoFile, int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var returnCategoryId = categoryId ?? model.CategoryId;
            if (!ModelState.IsValid)
            {
                await LoadInstructorsAsync(companyId.Value, model.InstructorId);
                ViewBag.CategoryId = returnCategoryId;
                return View(model);
            }

            model.Logo = await _fileService.UpdateFileAsync(logoFile, model.Logo, CourseLogoFolder);

            var result = await _courseService.UpdateAsync(model, companyId.Value);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { categoryId = returnCategoryId });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            await LoadInstructorsAsync(companyId.Value, model.InstructorId);
            ViewBag.CategoryId = returnCategoryId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignInstructor(int id, int? categoryId, int? instructorId, string? returnView = null)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var result = await _courseService.UnassignInstructorAsync(id, companyId.Value);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (string.Equals(returnView, "instructor", StringComparison.OrdinalIgnoreCase) && instructorId.HasValue)
            {
                return RedirectToAction("Details", "Instractor", new { area = "Company", id = instructorId.Value });
            }

            return RedirectToAction(nameof(Details), new { id, categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id, int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            var result = await _courseService.TogglePublishAsync(id, companyId.Value);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { categoryId = categoryId ?? course.CategoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            var result = await _courseService.DeleteAsync(id, companyId.Value);
            if (result.IsSuccess)
            {
                if (!string.IsNullOrEmpty(course.Logo))
                    _fileService.DeleteFile(course.Logo);

                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { categoryId = categoryId ?? course.CategoryId });
        }

        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }

        private async Task LoadInstructorsAsync(int companyId, int? selectedInstructorId = null)
        {
            var instructors = await _instructorServices.GetListInstructorAsync(companyId);
            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName", selectedInstructorId);
        }
    }
}
