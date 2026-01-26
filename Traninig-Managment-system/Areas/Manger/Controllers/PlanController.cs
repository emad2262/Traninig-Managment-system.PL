using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.ModelVm;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    public class PlanController : Controller
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }
        // GET: Manager/Plan
        public async Task<IActionResult> Index()
        {
            var plans = await _planService.GetAllAsync();
            return View(plans);
        }

        // GET: Manager/Plan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var plan = await _planService.GetByIdAsync(id);

            if (plan is null)
                return NotFound();

            return View(plan);
        }

        // GET: Manager/Plan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Manager/Plan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plan plan)
        {
            if (!ModelState.IsValid)
                return View(plan);
            

            var result = await _planService.AddAsync(plan);

            if (result)
            {
                TempData["Success"] = "تم إضافة الخطة بنجاح";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "حدث خطأ أثناء إضافة الخطة";
            return View(plan);
        }

        // GET: Manager/Plan/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _planService.GetByIdAsync(id);

            if (plan is null)
                return NotFound();

            return View(plan);
        }

        // POST: Manager/Plan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plan plan)
        {
            if (id != plan.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(plan);

            try
            {
                var result = await _planService.UpdateAsync(plan);

                if (result)
                {
                    TempData["Success"] = "تم تحديث الخطة بنجاح";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return View(plan);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _planService.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] = "success deleted plat";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "failed daleted plan";
            return RedirectToAction(nameof(Index));
        }
    }
}
//////////////////////////////////
///
