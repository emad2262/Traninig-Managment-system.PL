using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CertificatesController : Controller
    {
        private readonly ICompanyCertificateService _certificateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CertificatesController(
            ICompanyCertificateService certificateService,
            UserManager<ApplicationUser> userManager)
        {
            _certificateService = certificateService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status = "pending")
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var parsedStatus = ParseStatus(status);
            var certificates = await _certificateService.GetCertificatesAsync(companyId.Value, parsedStatus);
            ViewBag.CurrentStatus = status ?? "all";

            return View(certificates);
        }

        public async Task<IActionResult> Details(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var certificate = await _certificateService.GetCertificateDetailsAsync(companyId.Value, id);
            if (certificate == null) return NotFound();

            return View(certificate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(CompanyCertificateIssueVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null) return Unauthorized();

            var certificate = await _certificateService.GetCertificateDetailsAsync(user.CompanyId.Value, model.CertificateId);
            if (certificate == null) return NotFound();

            var certificateUrl = Url.Action(
                "Certificate",
                "EmployeeCourses",
                new { area = "Employee", courseId = certificate.CourseId },
                Request.Scheme);

            var result = await _certificateService.IssueCertificateAsync(
                user.CompanyId.Value,
                model,
                user.Id,
                certificateUrl);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = model.CertificateId });
        }

        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }

        private static CertificateStatus? ParseStatus(string? status)
        {
            return status?.Trim().ToLowerInvariant() switch
            {
                "issued" => CertificateStatus.Issued,
                "revoked" => CertificateStatus.Revoked,
                "all" => null,
                _ => CertificateStatus.PendingCompanyApproval
            };
        }
    }
}
