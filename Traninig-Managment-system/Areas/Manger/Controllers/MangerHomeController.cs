using Microsoft.AspNetCore.Mvc.Rendering;


namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    [Authorize(Roles = SD.SuperAdmin)]

    public class MangerHomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MangerHomeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var usersWithRoles = new List<UserWithRoleVm>();


            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                usersWithRoles.Add(new UserWithRoleVm
                {
                    Id = user.Id,
                    Name = user.UserName!,
                    Email = user.Email!,
                    Roles = roles.ToList(),
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
                });
            }

         

            return View(usersWithRoles);
        }

        public async Task<IActionResult> ChangeRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);//بجيب الرول الحالي للمستخدم
            var allRoles = await _roleManager.Roles.ToListAsync(); //بجيب كل الرولات المتاحة عشان اعرضها في الدروب داون

            var model = new ChangeRoleVm
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CurrentRole = userRoles.FirstOrDefault(),

                Roles = allRoles.Select(r => new SelectListItem
                {
                    Text = r.Name!,
                    Value = r.Name!,
                    Selected = r.Name == userRoles.FirstOrDefault()
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(ChangeRoleVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // إزالة كل الأدوار القديمة
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove current roles.");
                    return View(model);
                }
            }

            // إضافة الرول الجديد
            var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to assign the new role.");
                return View(model);
            }

            TempData["Success"] = "Role updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Text = r.Name!,
                Value = r.Name!
            }).ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserVm model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _roleManager.Roles.ToListAsync();
                ViewBag.Roles = roles.Select(r => new SelectListItem
                {
                    Text = r.Name!,
                    Value = r.Name!
                }).ToList();

                return View(model);

            }
            var user = new ApplicationUser
            {
                UserName = model.Name,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }
                    TempData["Success"] = "User created successfully";
                    return RedirectToAction(nameof(Index));
                }

                // إضافة الـ errors للـ ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var rolesOnError = await _roleManager.Roles.ToListAsync();
                ViewBag.Roles = rolesOnError.Select(r => new SelectListItem
                {
                    Text = r.Name!,
                    Value = r.Name!
                }).ToList();
            }
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId is required");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // حماية: ممنوع تحذف نفسك
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
                return BadRequest("You cannot delete your own account.");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest("Delete failed");

            return Ok(new { message = "User deleted successfully" });
        }

    }
}

