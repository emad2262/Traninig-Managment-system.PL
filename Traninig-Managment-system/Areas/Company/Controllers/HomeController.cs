
namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class HomeController : Controller
    {
        private readonly ICategoryCourseServices _categoryCourseServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ICategoryCourseServices categoryCourseServices,
            UserManager<ApplicationUser> userManager)
        {
            _categoryCourseServices = categoryCourseServices;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var homeVm =await _categoryCourseServices.GetCompanyHomeDataAsync(companyId);
            return View(homeVm);
        }

    }
}
