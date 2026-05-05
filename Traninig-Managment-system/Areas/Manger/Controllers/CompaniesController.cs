namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    [Authorize(Roles = SD.SuperAdmin)]
    public class CompaniesController : Controller
    {
        private readonly IManagerAreaService _managerAreaService;

        public CompaniesController(IManagerAreaService managerAreaService)
        {
            _managerAreaService = managerAreaService;
        }

        public async Task<IActionResult> Index(string? search = null)
        {
            var companies = await _managerAreaService.GetCompaniesAsync(search);
            ViewBag.Search = search;
            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var company = await _managerAreaService.GetCompanyDetailsAsync(id);
            return company == null ? NotFound() : View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(CreateCompanyNotificationVm model)
        {
            var result = await _managerAreaService.SendNotificationAsync(model);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.CompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRenewalReminder(int companyId)
        {
            var result = await _managerAreaService.SendRenewalReminderAsync(companyId);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = companyId });
        }
    }
}
