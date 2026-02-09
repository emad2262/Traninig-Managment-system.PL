namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    public class LessonController : Controller
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILessonServices _lessonServices;

        public LessonController(UserManager<ApplicationUser> userManager,ILessonServices lessonServices)
        {
            _userManager = userManager;
            _lessonServices = lessonServices;
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

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lessons");


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
                companyId, courseId, dto);

            return RedirectToAction(nameof(LessonDisplay), new { courseId });
        }


    }
}
