
using Traninig_Managment_system.BLL.Dtos.category;
using Traninig_Managment_system.View_Model.Company.Company_Category;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _userManager = userManager;
        }

        private async Task<int?> GetCompanyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.CompanyId;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null)
                return Unauthorized();

            var categories = await _categoryService.GetAllCategoyr(companyId.Value);

            var model = categories.Select(c => new CategoryDisplayVm
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                TotalCourses = c.TotalCourses
            }).ToList();

            return View(model);
        }

        //public async Task<IActionResult> Details(int id)
        //{
        //    var companyId = await GetCompanyIdAsync();
        //    if (companyId is null)
        //        return Unauthorized();

        //    var category = await _categoryService.GetCategoryByIdAsync(companyId.Value, id);
        //    if (category is null)
        //        return NotFound();

        //    var model = new CategoryDetailsVm
        //    {
        //        CategoryId = category.CategoryId,
        //        CategoryName = category.CategoryName,
        //        CategoryDescription = category.CategoryDescription,
        //        CreatedAt = category.CreatedAt,
        //        TotalCourse = category.TotalCourse,
        //        Courses = category.CourseListDtos.Select(c => new CategoryCourseVM
        //        {
        //            Id = c.Id,
        //            Title = c.Title,
        //            Logo = c.logo,
        //            DurationInHours = c.DurationInHours,
        //            IsPublished = c.IsPublished
        //        }).ToList()
        //    };

        //    return View(model);
        //}


        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCategoryVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var companyId = await GetCompanyIdAsync();
            if (companyId is null)
                return Unauthorized();

            var dto = new CreateCategoryDto
            {
                Name = model.Name,
                Description = model.Description
            };

            await _categoryService.CreateCategoryAsync(dto, companyId.Value);

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null)
                return Unauthorized();

            var category = await _categoryService.GetCategoryByIdAsync(companyId.Value, id);
            if (category is null)
                return NotFound();

            var model = new EditCategoryVm
            {
                Id = category.CategoryId,
                Name = category.CategoryName,
                Description = category.CategoryDescription
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCategoryVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var companyId = await GetCompanyIdAsync();
            if (companyId is null)
                return Unauthorized();

            var dto = new UpdateCategoryDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };

            try
            {
                await _categoryService.UpdateCategoryAsync(companyId.Value, dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId is null)
                return Unauthorized();

            try
            {
                var isDeleted = await _categoryService.DeleteCategoryAsync(id, companyId.Value);

                if (!isDeleted)
                    TempData["Error"] = "Category was not found.";
                else
                    TempData["Success"] = "Category deleted successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
