using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity;
using Traninig_Managment_system.BLL.Services;
using Traninig_Managment_system.BLL.Services.classes;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class HomeController : Controller
    {
        private readonly IEmployeeManagementService _employeeManagementService;
        private readonly ICourseServices _courseServices;
        private readonly ICategoryService _categoryService;
        private readonly IInstructorServices _instructorServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IEmployeeManagementService employeeManagementService,ICourseServices courseServices,
            ICategoryService categoryService,IInstructorServices instructorServices, UserManager<ApplicationUser> userManager)
        {
            _employeeManagementService = employeeManagementService;
            _courseServices = courseServices;
            _categoryService = categoryService;
            _instructorServices = instructorServices;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
                var companyId = user.CompanyId.Value;
            
            var model = new CompanyDisplayVm
            {
                EmployeeCount = await _employeeManagementService.EmployeeCount(companyId),

                CourseCount = await _courseServices.CourseCount(companyId),

                PublishedCourseCount = await _courseServices.PublishedCourseCount(companyId),

                CategoryCount = await _categoryService.CategoryCount(companyId),

                InstructorCount = await _instructorServices.InstructorCount(companyId)
            };
            return View(model);
        }
    }
}
