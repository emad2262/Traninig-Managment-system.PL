using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.ModelVm;
using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.Utality;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Traninig_Managment_system.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller // Inherit from Controller to use View()
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICompanyServices _companyServices;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,ICompanyServices companyServices, SignInManager<ApplicationUser> signInManager,IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _companyServices = companyServices;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard", new { area = "Company" });

            return View();
        }

        // ==================== Register (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterCompanyVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1️⃣ Check if email exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered");
                return View(model);
            }

            // 2️⃣ Create Company
            var company = new Traninig_Managment_system.DAL.Model.Company
            {
                Name = model.CompanyName,
                Email = model.Email,
                IsActive = true,
                SubscriptionStart = DateTime.UtcNow
            };

            company = await _companyServices.CreateAsync(company); // ✔️

            // 3️⃣ Create User
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone,
                CompanyId = company.Id,
                EmailConfirmed = true,      

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, SD.Company);


            TempData["Success"] = "Account created successfully!";
            return RedirectToAction("Index", "Home", new { area = "Company" });
        }

        // ==================== Login ====================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectBasedOnRole();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectBasedOnRole();
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        // ==================== Logout ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ==================== Helper ====================
        private IActionResult RedirectBasedOnRole()
        {

            if (User.IsInRole(SD.SuperAdmin))
                return RedirectToAction("Index", "DashBoard", new { area = "Manger" });

            if (User.IsInRole(SD.Company))
                return RedirectToAction("Index", "Home", new { area = "Company" });

            if (User.IsInRole(SD.Instructor))
                return RedirectToAction("DashBoard", "Home", new { area = "Instractor" });

            if (User.IsInRole(SD.Employee))
                return RedirectToAction("DashBoard", "Home", new { area = "Employee" });

            return RedirectToAction("Index", "Home");
        }

        //confirm account 
        public async Task<IActionResult> ConfirmEmail(string UserId, string tokeen)
        {

            var applicationUser = await _userManager.FindByIdAsync(UserId);

            if (applicationUser == null)
            {
                return RedirectToAction("NotFoundPage", "Home", new { area = "Customer" });
            }

            var result = await _userManager.ConfirmEmailAsync(applicationUser, tokeen);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        ///log out
        ///
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            TempData["notification"] = "log out";
            return RedirectToAction("Index", "Home", new { Area = "Customer" });
        }
    }
}
