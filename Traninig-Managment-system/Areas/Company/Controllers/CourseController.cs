using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services.classes;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    public class CourseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly ICourseServices _courseService;
        private readonly IInstructorServices _instructorServices;

        public CourseController(UserManager<ApplicationUser> userManager, IFileService fileService,
            ICourseServices courseServices,IInstructorServices instructorServices)
        {
            _userManager = userManager;
            _fileService = fileService;
            _courseService = courseServices;
            _instructorServices = instructorServices;
        }

        // ===============================
        // 🔹 Helpers
        // ===============================
        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }

        // ===============================
        // 🔹 Index
        // ===============================
        public async Task<IActionResult> Index(int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var courses = await _courseService
                .GetAllInCategoryAsync(companyId.Value, categoryId);

            ViewBag.CategoryId = categoryId;

            return View(courses);
        }
        // ===============================
        // 🔹 Create (GET)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Create(int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var instructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName");

            // 2. تهيئة الموديل برقم القسم عشان الـ Hidden Input ياخده
            var model = new CourseDto
            {
                CategoryId = categoryId
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseDto model, IFormFile? logoFile)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                // لو الداتا فيها غلط، بنعيد تحميل قائمة المدربين عشان الـ Dropdown ميفضاش
                var listInstructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
                ViewBag.Instructors = new SelectList(listInstructors, "Id", "FullName");
                return View(model);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                var filePath = await _fileService.UploadFileAsync(logoFile, "course-logos");
                model.Logo = filePath;
            }

            var result = await _courseService.CreateCourseAsync(model, companyId.Value);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم إنشاء الكورس بنجاح!";
                return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
            }

            // لو فيه خطأ بيزنس (زي تاريخ النهاية قبل البداية)
            ModelState.AddModelError("", result.Message);

            // إعادة تحميل القوائم قبل الرجوع للفيو
            var instructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName");

            return View(model);

        }

        // ===============================
        // 🔹 Edit (GET)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            var instructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName", course.InstructorId);
            ViewBag.CategoryId = categoryId;

            return View(course);
        }

        // ===============================
        // 🔹 Edit (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseDto model, IFormFile? logoFile, int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                var instructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
                ViewBag.Instructors = new SelectList(instructors, "Id", "FullName", model.InstructorId);
                ViewBag.CategoryId = categoryId;
                return View(model);
            }

            // معالجة الصورة — لو رفع جديدة بنبدل، لو ملوش بنسيب القديمة
            model.Logo = await _fileService.UpdateFileAsync(logoFile, model.Logo, "course-logos");

            var result = await _courseService.UpdateAsync(model, companyId.Value);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم تعديل الكورس بنجاح!";
                return RedirectToAction(nameof(Index), new { categoryId });
            }

            ModelState.AddModelError("", result.Message);

            var inst = await _instructorServices.GetListInstructorAsync(companyId.Value);
            ViewBag.Instructors = new SelectList(inst, "Id", "FullName", model.InstructorId);
            ViewBag.CategoryId = categoryId;
            return View(model);
        }

        // ===============================
        // 🔹 Delete (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int categoryId)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            // جيب الـ Logo path قبل الحذف عشان تحذف الصورة
            var course = await _courseService.GetByIdAsync(id, companyId.Value);
            if (course == null) return NotFound();

            var result = await _courseService.DeleteAsync(id, companyId.Value);

            if (result.IsSuccess)
            {
                // احذف الصورة من السيرفر لو موجودة
                if (!string.IsNullOrEmpty(course.Logo))
                    _fileService.DeleteFile(course.Logo);

                TempData["SuccessMessage"] = "تم حذف الكورس بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { categoryId });
        }
    }
}
