
using Traninig_Managment_system.BLL.Services.Interfaces;


namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeManagementService _employeeServices;
        

        public EmployeeController(UserManager<ApplicationUser> userManager,
            IEmployeeManagementService employeeServices)
        {
            _userManager = userManager;
            _employeeServices = employeeServices;
           
        }

        [HttpGet]
        public async Task<IActionResult> Index(string name = "", int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || currentUser.CompanyId == null)
            {
                return Unauthorized();
            }
            var allEmployees = await _employeeServices.GetEmployeesWithCoursesCountAsync(currentUser.CompanyId.Value);
            //filtering by name if provided
            if (!string.IsNullOrEmpty(name))
            {
                allEmployees = allEmployees.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var vm = new EmployeeIndexVm
            {
                Employees = allEmployees,
                Name = name,
                CurrentPage = page,

            };

            return View(vm);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddEmployeeVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.CompanyId == null)
                return Unauthorized();

            var result = await _employeeServices.AddEmployeeAsync(model, currentUser.CompanyId.Value);

            if (!result.IsSuccess)
            {
                // نجمع كل الأخطاء في رسالة واحدة
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || currentUser.CompanyId == null)
            {
                return Unauthorized();
            }
            var companyId = currentUser.CompanyId.Value;
            var employee = await _employeeServices.GetEmployeeByIdAsync(companyId,Id);
            if (employee == null)
                return NotFound("Employee not found");
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> AssignCourse(int employeeId, string? search = null, int? categoryId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.CompanyId == null)
                return Unauthorized();

            var vm = await _employeeServices.GetAssignCourseDataAsync(currentUser.CompanyId.Value, employeeId, search, categoryId);
            if (vm == null)
                return NotFound("Employee not found");

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCourse(AssignCourseVm model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.CompanyId == null)
                return Unauthorized();

            var companyId = currentUser.CompanyId.Value;

            if (model.SelectedCourseIds == null || !model.SelectedCourseIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one course to assign.");
                var reload = await _employeeServices.GetAssignCourseDataAsync(companyId, model.EmployeeId, model.Search, model.CategoryId);
                return View(reload);
            }

            var result = await _employeeServices.AssignCoursesToEmployeeAsync(companyId, model.EmployeeId, model.SelectedCourseIds);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                var reload = await _employeeServices.GetAssignCourseDataAsync(companyId, model.EmployeeId, model.Search, model.CategoryId);
                return View(reload);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { Id = model.EmployeeId });
        }

    }
}

