using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    [Area("Employee")]

    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeLessonServices _employeeLessonServices;

        public HomeController(UserManager<ApplicationUser> userManager,IEmployeeLessonServices employeeLessonServices)
        {
            _userManager = userManager;
            _employeeLessonServices = employeeLessonServices;
        }
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            var vm = await _employeeLessonServices
                .GetEmployeeDashboardAsync(user.Id);

            return View(vm);
        }
    }
}
