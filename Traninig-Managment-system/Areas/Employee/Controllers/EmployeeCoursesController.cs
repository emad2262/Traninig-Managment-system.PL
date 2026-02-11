using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class EmployeeCoursesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeLessonServices _employeeLessonServices;

        public EmployeeCoursesController(
            UserManager<ApplicationUser> userManager,
            IEmployeeLessonServices employeeLessonServices)
        {
            _userManager = userManager;
            _employeeLessonServices = employeeLessonServices;
        }

        public async Task<IActionResult> Details(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var vm = await _employeeLessonServices
                .GetCourseLessonsForEmployeeAsync(user.Id, courseId);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
    }
}
