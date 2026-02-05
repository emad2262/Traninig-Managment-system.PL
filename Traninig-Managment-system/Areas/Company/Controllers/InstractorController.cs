using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.ModelVm;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    public class InstractorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInstructorServices _instructorServices;

        public InstractorController(UserManager<ApplicationUser> userManager,IInstructorServices instructorServices)
        {
            _userManager = userManager;
            _instructorServices = instructorServices;
        }

        public async Task<IActionResult> Index(string? name, int page = 1)
        {
            // get company id الى عامله تسجيل دخول
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            // get company id
            var companyId = user.CompanyId.Value;
            var listInstructors = await _instructorServices.GetListInstructorAsync(companyId);
            /////filter by name
            if (!string.IsNullOrEmpty(name))
            {
                listInstructors = listInstructors.Where(e => e.FullName!.ToLower().Contains(name.ToLower()));
            }
            //////////pagination//////////
            int pageSize = 4;
            var totalpage = (int)Math.Ceiling((double)listInstructors.Count() / pageSize);
            listInstructors = listInstructors.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;
            
            var model = new instructorIndexVm
            {
                Instructors = listInstructors,
                CurrentPage = page,
                TotalPages = totalpage,
                Name = name
            };
            return View(model);

        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreaeInstructorVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }

            var companyId = user.CompanyId.Value;
            var result = await _instructorServices.AddInstructor(model, companyId);
            if (result)
            {
                return RedirectToAction("Index", "Instractor", new { area = "Company" });
            }
            ModelState.AddModelError(string.Empty, "Failed to add employee");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var employee = await _instructorServices.GetinsturactorByIdAsync(id, companyId);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditInstructorVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var result = await _instructorServices.EditinsturactorByIdAsync(model, companyId);
            if (result)
            {
                return RedirectToAction("Index", "Instractor", new { area = "Company" });
            }
            ModelState.AddModelError(string.Empty, "Failed to edit employee");
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            // get company id الى عامله تسجيل دخول 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user.");
            }
            var companyId = user.CompanyId.Value;
            var result = await _instructorServices.Delete(id, companyId);
            if (result)
            {
                return RedirectToAction("Index", "Instractor", new { area = "Company" });
            }
            return BadRequest("Failed to toggle active status");
        }
    }
    
}
