namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    [Authorize(Roles = SD.SuperAdmin)]

    public class DashBoardController : Controller
    {
        private readonly IDashboardService _dashboard;
        private readonly StatisticsManager _statisticsManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DashBoardController(IDashboardService dashboard, StatisticsManager statisticsManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dashboard = dashboard;
           _statisticsManager = statisticsManager;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {

            var companies = await _dashboard.GetAllCompaniesWithDetailsAsync();

            var totalCompanies = await _context.companies.CountAsync();
            var totalEmployees = await _context.employees.CountAsync();
            var totalCourses = await _context.courses.CountAsync();

            // pagination can be added here later
            int pageSize = 4;
            var totalpage = (int)Math.Ceiling((double)companies.Count() / pageSize);
            companies = companies.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;  // ⬅️ ضيف ده
            ////
            DashboardVm dashboard = new DashboardVm
            {
                TotalEmployees = totalEmployees,
                TotalCourses = totalCourses,
                ExpiringSoon = companies.Count(c => c.SubscriptionEnd <= DateTime.Now.AddDays(36)),
                totalpage = totalpage,
                companyList = companies.ToList(),
            };
            return View(dashboard);

        }
        public async Task<IActionResult> Create()
        {
            await PopulateViewBags();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm model)
        {
            // ⚠️ مهم: لازم تعيد الـ ViewBag لو رجعت الـ View
            if (!ModelState.IsValid)
            {
                await PopulateViewBags();
                return View(model);
            }

            if (model.SelectedRole == SD.Company && model.SelectedPlanId == null)
            {
                ModelState.AddModelError("SelectedPlanId", "Plan is required for company");
                await PopulateViewBags();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Name,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                await PopulateViewBags();
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.SelectedRole))
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

            if (model.SelectedRole == SD.Company)
            {
                var company = new DAL.Model.Company
                {
                    Name = model.Name,
                    Email = model.Email,
                    PlanId = model.SelectedPlanId
                };
                _context.companies.Add(company);
                await _context.SaveChangesAsync();

                user.CompanyId = company.Id;
                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] = "User created successfully";
            return RedirectToAction(nameof(Index));
        }

        // Helper method لتجنب التكرار
        private async Task PopulateViewBags()
        {
            var roles = await _roleManager.Roles.ToListAsync();//هنجيب كل الرولر من الرول مانجير 
            ViewBag.roles = roles.Select(e => new SelectListItem //هبدا الف علها بقه عشان اعرضها 
            {
                Text = e.Name,
                Value = e.Name
            }).ToList();

            var plans = _context.plans.ToList();
            ViewBag.plans = plans.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            }).ToList();

        }
    
    public async Task<IActionResult> Chart()
    {
        var statistics = await _statisticsManager.GetDashboardStatisticsAsync();
        return View(statistics);
    }

    }
}
