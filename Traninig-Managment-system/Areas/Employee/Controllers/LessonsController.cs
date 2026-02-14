using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class LessonsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeLessonServices _employeeLessonServices;

        public LessonsController(
            UserManager<ApplicationUser> userManager,
            IEmployeeLessonServices employeeLessonServices)
        {
            _userManager = userManager;
            _employeeLessonServices = employeeLessonServices;
        }


        [HttpPost]
        public async Task<IActionResult> MarkCompleted(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);

            await _employeeLessonServices
                .MarkLessonAsCompletedAsync(user.Id, lessonId);

            return RedirectToAction("Watch", new { lessonId });
        }
    }

}
