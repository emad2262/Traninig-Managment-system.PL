namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    [Authorize(Roles = SD.SuperAdmin)]
    public class PlanController : Controller
    {
        private readonly IManagerAreaService _managerAreaService;

        public PlanController(IManagerAreaService managerAreaService)
        {
            _managerAreaService = managerAreaService;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _managerAreaService.GetPlansAsync();
            return View(plans);
        }

        public async Task<IActionResult> Details(int id)
        {
            var plan = await _managerAreaService.GetPlanAsync(id);
            return plan == null ? NotFound() : View(plan);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ManagerPlanVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManagerPlanVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _managerAreaService.CreatePlanAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _managerAreaService.GetPlanAsync(id);
            return plan == null ? NotFound() : View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManagerPlanVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _managerAreaService.UpdatePlanAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _managerAreaService.DeletePlanAsync(id);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
