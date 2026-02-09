using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.ModelVm;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]

    public class CategoryController : Controller
    {
        private readonly ICategoryServices _categoryServices;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ICategoryServices categoryServices,UserManager<ApplicationUser> userManager)
        {
            _categoryServices = categoryServices;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user= await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("user in not registeration");
            }
            var companyid =user.CompanyId.Value;

            var categories = await _categoryServices.GetCategoriesInCompany(companyid);
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryVm model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.CompanyId.HasValue)
                return Unauthorized();

            var companyId = user.CompanyId.Value;
            if (!ModelState.IsValid)
            {
                var categories = await _categoryServices.GetCategoriesInCompany(companyId);
                return View("Index", categories);
            }
            await _categoryServices.AddCategories(companyId, model);
            TempData["SuccessMessage"] = "Category created successfully";


            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int categoryid)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.CompanyId.HasValue)
                return Unauthorized();

            var companyId = user.CompanyId.Value;

            await _categoryServices.DeleteCategories(categoryid,companyId);
            return RedirectToAction(nameof(Index));

        }
        //=================================================================================================//


    }

}
