
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    public class InstractorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInstructorServices _instructorServices;

        public InstractorController(UserManager<ApplicationUser> userManager, IInstructorServices instructorServices)
        {
            _userManager = userManager;
            _instructorServices = instructorServices;
        }

        private async Task<int?> GetCompanyId()
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
            var companyId = await GetCompanyId();
            if (companyId == null) return NotFound();

            var instructors = await _instructorServices.GetListInstructorAsync(companyId.Value);
            return View(instructors);

        }
        //============================/
        //===========  Details  ============/
        //============================/

        public async Task<IActionResult> Details(int id)
        {
            var companyId = await GetCompanyId();
            if (companyId == null) return NotFound();

            var instructor = await _instructorServices.GetInstructorDetailsAsync(companyId.Value, id);
            if (instructor == null) return NotFound();

            return View(instructor);

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
        public async Task<IActionResult> Create(CreateInstructorVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var companyId = await GetCompanyId();
            if (companyId == null) return NotFound();
            var result = await _instructorServices.CreateInstructorAsync(companyId.Value, model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            TempData["SuccessMessage"] = "Instructor created successfully.";
            return RedirectToAction(nameof(Index));
        }
        //============================/
        //======  delete ========/
        //============================/
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCompanyId();
            if (companyId == null) return NotFound();

            var result = await _instructorServices.DeleteInstructorAsync(companyId.Value, id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = "Instructor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}