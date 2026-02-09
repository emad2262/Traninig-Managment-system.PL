namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class HomeController : Controller
    {
      
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyServices _companyServices;

        public HomeController(UserManager<ApplicationUser> userManager, ICompanyServices companyServices)
        {
            
            _userManager = userManager;
            _companyServices = companyServices;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("user in not registeration");
            }
            var companyid = user.CompanyId.Value;
            var vm = await _companyServices.GetCompanyOverviewAsync(companyid);

            return View(vm);

        }
        

    }
}
