namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    [Authorize(Roles = SD.SuperAdmin)]
    public class DashBoardController : Controller
    {
        private readonly IManagerAreaService _managerAreaService;

        public DashBoardController(IManagerAreaService managerAreaService)
        {
            _managerAreaService = managerAreaService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _managerAreaService.GetDashboardAsync();
            return View(vm);
        }

        public IActionResult ChatHub()
        {
            return View();
        }
    }
}
