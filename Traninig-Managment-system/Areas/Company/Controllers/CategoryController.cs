

using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]

    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ICategoryService categoryService,UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user= await _userManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null)
                return Unauthorized();

            var categories = await _categoryService.GetCategoriesByCompanyAsync(user.CompanyId.Value);
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.CompanyId == null)
                return Unauthorized();

            // 1. Security: إجبار الموديل إنه ياخد رقم شركة اليوزر الحالي (عشان نمنع الاختراق)
            model.CompanyId = user.CompanyId.Value;

            // 2. التأكد من الـ Validation
            if (!ModelState.IsValid)
            {
                // لو فيه خطأ، بنخزنه في TempData عشان نعرضه في نفس صفحة الـ Index
                TempData["ErrorMessage"] = "تأكد من إدخال اسم القسم بشكل صحيح.";
                return RedirectToAction(nameof(Index));
            }

            // 3. محاولة الحفظ
            var isSuccess = await _categoryService.CreateCategoryAsync(model);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "تم إضافة القسم بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "يوجد قسم بهذا الاسم بالفعل.";
            }

            // 4. دايماً في الـ POST اللي في نفس الصفحة بنعمل Redirect للـ GET
            return RedirectToAction(nameof(Index));
        }
        //public async Task<IActionResult> Delete(int categoryid)
        //{

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null || !user.CompanyId.HasValue)
        //        return Unauthorized();

        //    var companyId = user.CompanyId.Value;

        //    await _categoryServices.DeleteCategories(categoryid,companyId);
        //    return RedirectToAction(nameof(Index));

        //}
        //=================================================================================================//


    }

}
