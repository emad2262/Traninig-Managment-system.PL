
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
            return View();

        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id)
        {
            return View();


        }

        public async Task<IActionResult> Details(int Id)
        {
            return View();

        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            return View();

        }

    }
}

