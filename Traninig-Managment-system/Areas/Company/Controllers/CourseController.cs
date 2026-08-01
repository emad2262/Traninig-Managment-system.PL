
using Traninig_Managment_system.BLL.Dtos.Course;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.View_Model.Company.Compny_Course;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CourseController : Controller
    {
        private const string LogoFolder = "images/courses";
        private static readonly string[] AllowedLogoExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxLogoBytes = 2 * 1024 * 1024; // 2 MB

        private readonly ICourseServices _courseServices;
        private readonly ICategoryService _categoryService;
        private readonly IInstructorServices _instractorService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(
            ICourseServices courseServices,
            ICategoryService categoryService,
            IInstructorServices instractorService,
            IFileService fileService,
            UserManager<ApplicationUser> userManager)
        {
            _courseServices = courseServices;
            _categoryService = categoryService;
            _instractorService = instractorService;
            _fileService = fileService;
            _userManager = userManager;
        }

        // =============================================================
        //  INDEX
        // =============================================================
        public async Task<IActionResult> Index(int? categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var courses = categoryId.HasValue && categoryId > 0
                ? await _courseServices.GetAllCoursesInCategoryAsync(companyId.Value, categoryId.Value)
                : await _courseServices.GetCompanyCoursesAsync(companyId.Value);

            var model = new CourseIndexVM
            {
                SelectedCategoryId = categoryId,
                Categories = await BuildCategoryOptionsAsync(companyId.Value),
                Courses = courses.Select(c => new CourseListVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Logo = c.logo,
                    DurationInHours = c.DurationInHours,
                    LessonCount = c.LessonCount,
                    IsPublished = c.IsPublished
                }).ToList()
            };

            return View(model);
        }

        // =============================================================
        //  DETAILS
        // =============================================================
        public async Task<IActionResult> Details(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var course = await _courseServices.GetCourseDetailsAsync(companyId.Value, id);
            if (course is null) return NotFound();

            var model = new CourseDetailsVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = course.logo,
                DurationInHours = course.DurationInHours,
                IsPublished = course.IsPublished,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                InstructorName = course.InstructorName,
                CategoryName = course.CategoryName,
                Lessons = course.LessonsList.Select(l => new CourseLessonVM
                {
                    Id = l.Id,
                    Title = l.Title,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    PdfUrl = l.PdfUrl,
                    Order = l.Order,
                    CreatedAt = l.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        // =============================================================
        //  CREATE
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var model = new CreateCourseVM
            {
                Categories = await BuildCategoryOptionsAsync(companyId.Value),
                Instructors = await BuildInstructorOptionsAsync(companyId.Value)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseVM model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            ValidateLogo(model.Logo);

            if (model.EndDate < model.StartDate)
                ModelState.AddModelError(nameof(model.EndDate), "The end date cannot be earlier than the start date.");


            var logoPath = await _fileService.UploadFileAsync(model.Logo, LogoFolder, AllowedLogoExtensions, MaxLogoBytes);


            var dto = new CreateCourseDto
            {
                Title = model.Title,
                Description = model.Description,
                logo = logoPath,
                DurationInHours = model.DurationInHours,
                IsPublished = model.IsPublished,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CategoryId = model.CategoryId,
                InstructorId = model.InstructorId
            };

            var result = await _courseServices.CreateCourseAsync(dto, companyId.Value);

            if (!result.IsSuccess)
            {
                // الحفظ فشل — امسح الصورة اللي اترفعت عشان متفضلش زبالة على الديسك
                _fileService.DeleteFile(logoPath);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = result.Data });
        }

        // =============================================================
        //  EDIT
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var course = await _courseServices.GetCourseForEditAsync(companyId.Value, id);
            if (course is null) return NotFound();

            var model = new UpdateCourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                CurrentLogo = course.Logo,
                DurationInHours = course.DurationInHours,
                IsPublished = course.IsPublish,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId,
                Categories = await BuildCategoryOptionsAsync(companyId.Value),
                Instructors = await BuildInstructorOptionsAsync(companyId.Value)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCourseVM model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            ValidateLogo(model.Logo);

            if (model.EndDate < model.StartDate)
                ModelState.AddModelError(nameof(model.EndDate), "The end date cannot be earlier than the start date.");


            // بيرجّع اللوجو القديم لو مرفعش جديد، وبيمسح القديم لو رفع
            var logoPath = await _fileService.UpdateFileAsync(model.Logo, model.CurrentLogo, LogoFolder, AllowedLogoExtensions
                , MaxLogoBytes,true);

            var dto = new UpdateCourseDto
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Logo = logoPath,
                DurationInHours = model.DurationInHours,
                IsPublish = model.IsPublished,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CategoryId = model.CategoryId,
                InstructorId = model.InstructorId
            };

            var result = await _courseServices.EditCourseAsync(dto, companyId.Value);

           
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // =============================================================
        //  DELETE
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var result = await _courseServices.DeleteCourseAsync(id, companyId.Value);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        //  TOGGLE PUBLISH
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id, string? returnUrl)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null) return Unauthorized();

            var result = await _courseServices.TogglePublishAsync(id, companyId.Value);

            if (result.IsSuccess)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result.Message;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        //  HELPERS
        // =============================================================
        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }

        private void ValidateLogo(IFormFile? logo)
        {
            if (logo is null || logo.Length == 0) return;

            var ext = Path.GetExtension(logo.FileName).ToLowerInvariant();

            if (!AllowedLogoExtensions.Contains(ext))
                ModelState.AddModelError("Logo", "Use a JPG, PNG, or WebP image.");

            if (logo.Length > MaxLogoBytes)
                ModelState.AddModelError("Logo", "The image must be under 2 MB.");
        }

        private async Task<List<SelectListItem>> BuildCategoryOptionsAsync(int companyId)
        {
            var categories = await _categoryService.GetAllCategoyr(companyId);

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList();
        }

        private async Task<List<SelectListItem>> BuildInstructorOptionsAsync(int companyId)
        {
            var instructors = await _instractorService.GetListInstructorAsync(companyId);

            return instructors.Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = i.FullName
            }).ToList();
        }

        
    }
}