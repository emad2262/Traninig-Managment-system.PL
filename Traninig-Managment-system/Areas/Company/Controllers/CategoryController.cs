
namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            return View();
            
        }

        public async Task<IActionResult> Details(int id)
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
