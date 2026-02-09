using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    public class CourseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseServices _courseServices;

        public CourseController(UserManager<ApplicationUser> userManager,ICourseServices courseServices)
        {
           _userManager = userManager;
           _courseServices = courseServices;
        }

        public async Task<IActionResult> CourseDisplay(int CategoryId)
        {
            var courses = await _courseServices.GetCourse(CategoryId);

            ViewBag.CategoryId = CategoryId; // لو هتحتاجه بعدين
            return View(courses);
       
        }

        public IActionResult CreateCourse(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CreateCourse(int categoryId, CreateCourseViewModel model)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = categoryId;

                return View(model);
            }

            string? logoPath = null;

            if (model.Logo != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(model.Logo.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Logo.CopyToAsync(stream);

                logoPath = fileName;
            }
            var bllModel = new CreateCourseVm
            {
                CourseName = model.CourseName,
                Description = model.Description,
                DurationInHours = model.DurationInHours,
                InstructorId = model.InstructorId,
                LogoPath = logoPath
            };

            await _courseServices.AddCourse(categoryId, bllModel);
            TempData["SuccessMessage"] = "Course added successfully";


            return RedirectToAction("CourseDisplay", new { CategoryId = categoryId });
        }
    }
}
