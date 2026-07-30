
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class InstractorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInstructorServices _instructorServices;

        public InstractorController(UserManager<ApplicationUser> userManager, IInstructorServices instructorServices)
        {
            _userManager = userManager;
            _instructorServices = instructorServices;
        }

        public async Task<int?> GetCompanyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return user.CompanyId;
        }
        //============================/
        //===========  Index  ============/
        //============================/

        public async Task<IActionResult> Index()
        {
            return View();


        }
        //============================/
        //===========  Details  ============/
        //============================/

        public async Task<IActionResult> Details(int id)
        {
            return View();


        }
        //============================/
        //=======  Create get ========/
        //============================/

        public async Task<IActionResult> Create()
        {
            return View();
        }

        //============================/
        //======  Create post ========/
        //============================/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int model)
        {
            return View();

        }
        //============================/
        //======  delete ========/
        //============================/
        public async Task<IActionResult> Delete(int id)
        {
            return View();

        }
    }
}
