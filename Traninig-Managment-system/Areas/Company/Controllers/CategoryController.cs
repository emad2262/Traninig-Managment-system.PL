using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
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
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var categories = await _categoryService.GetCategoriesByCompanyAsync(companyId.Value);
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var category = await _categoryService.GetCategoryByIdAsync(companyId.Value, id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            return View(new CreateCategoryVM { CompanyId = companyId.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVM model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            model.CompanyId = companyId.Value;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.CreateCategoryAsync(model, companyId.Value);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var category = await _categoryService.GetCategoryForEditAsync(companyId.Value, id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateCategoryVM model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            model.CompanyId = companyId.Value;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.UpdateCategoryAsync(model, companyId.Value);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null) return Unauthorized();

            var result = await _categoryService.DeleteCategoryAsync(id, companyId.Value);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }
    }
}
