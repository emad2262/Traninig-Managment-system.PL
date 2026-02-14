using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Instractor.Controllers
{
    [Area("Instractor")]
    public class InstructorLessonsController : Controller
    {
        private readonly ILessonServices _lessonServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorLessonsController(ILessonServices lessonServices,UserManager<ApplicationUser> userManager)
        {
            _lessonServices = lessonServices;
            _userManager = userManager;
        }
        public async Task<IActionResult> LessonDisplay(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var companyId = user.CompanyId.Value;

            var lessons = await _lessonServices
                .GetLessonsByCourseAsync(companyId, courseId);

            ViewBag.CourseId = courseId;
            return View(lessons);
        }
        public IActionResult CreateLessons(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(1073741824)] // السماح برفع ملفات حتى 1 جيجا (بالبايت)
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLessons(int courseId, LessonVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = courseId;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var companyId = user.CompanyId.Value;

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "lessons");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{model.File.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var dto = new CreateLessonVm
            {
                Title = model.Title,
                Description = model.Description,
                Order = model.Order,
                ContentUrl = "/uploads/lessons/" + fileName
            };

            await _lessonServices.AddLessonToCourseAsync(
                companyId,
                courseId,
                dto,
                user.Id);

            return RedirectToAction(
                "Details",
                "Home",
                new { area = "Instractor", id = courseId });
        }

        public async Task<IActionResult> EditLessons(int courseId,int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);

            var lesson = await _lessonServices
                .GetLessonForEditAsync(lessonId, courseId, user.Id);

            if (lesson == null)
                return NotFound();

            ViewBag.CourseId = courseId;
            return View(lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditLessons(int id, int courseId, EditLessonVm model)
        {
            // تأكد أن الـ ID اللي جاي في الـ URL هو نفسه اللي في الموديل
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = courseId;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            // نمرر model.Id للخدمة
            var result = await _lessonServices.EditLessonToCourseAsync(
                     model.Id,
                     courseId,
                     model,
                     user.Id);

            if (!result) return NotFound();

            TempData["Success"] = "Lesson updated successfully";

            // تأكد من اسم الـ Action والـ Controller في الـ Redirect
            return RedirectToAction("Details", "Home", new { area = "Instractor", id = courseId });
        }


    }
}
