

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeServices _employeeServices;

        public EmployeeController(UserManager<ApplicationUser> userManager, IEmployeeServices employeeServices)
        {
            _userManager = userManager;
            _employeeServices = employeeServices;
        }
      
        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            // get company id الى عامله تسجيل دخول
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            // get company id
            var companyId = user.CompanyId.Value;
            var employees = await _employeeServices.GetListEmployeeAsync(companyId);

            //////////pagination//////////
            int pageSize = 4;
            var totalpage = (int)Math.Ceiling((double)employees.Count() / pageSize);
            employees = employees.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;
            /////filter by name
            if (!string.IsNullOrEmpty(name))
            {
                employees = employees.Where(e => e.Name!.ToLower().Contains(name.ToLower()));
            }
            var model = new EmployeeIndexVm
            {
                ListEmployees = employees,
                CurrentPage = page,
                TotalPages = totalpage,
                Name = name
            };
            return View(model);

        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            var companyId = user.CompanyId.Value;
            var result = await _employeeServices.AddEmployee(model, companyId);
            if (result)
            {
                return RedirectToAction("Index", "Home", new { area = "Company" });
            }
            ModelState.AddModelError(string.Empty, "Failed to add employee");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var employee = await _employeeServices.GetEmployeeByIdAsync(id, companyId);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEmployeeVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var result = await _employeeServices.EditEmployeeAsync(model, companyId);
            if (result)
            {
                return RedirectToAction("Index", "Employee", new { area = "Company" });
            }
            ModelState.AddModelError(string.Empty, "Failed to edit employee");
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var result = await _employeeServices.Delete(id,companyId);
            if (result)
            {
                return RedirectToAction("Index", "Employee", new { area = "Company" });
            }
            return BadRequest("Failed to toggle active status");
        }
        [HttpGet]
        public async Task<IActionResult> AssignCourse(int employeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.CompanyId == null)
                return BadRequest();

            var vm = await _employeeServices
                .GetAssignCoursesForEmployeeAsync(employeeId, user.CompanyId.Value);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSingleCourse(int employeeId, int courseId)
        {
            await _employeeServices.AssignCourseToEmployeeAsync(courseId, employeeId);
            return RedirectToAction("AssignCourse", new { employeeId });
        }



    }
}

