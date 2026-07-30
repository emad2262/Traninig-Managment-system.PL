
using System.Threading.Tasks;
using Traninig_Managment_system.Areas.Customer.Controllers;

namespace Traninig_Managment_system.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller // Inherit from Controller to use View()
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepo _companyRepo;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,ICompanyRepo companyRepo
            , RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _companyRepo = companyRepo;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        // ==================== Register (put) ====================

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectBasedOnRole();

            return View(new RegisterVm());
        }

        // ==================== Register (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid)
                return View(model);


            // إنشاء المستخدم
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = false
            };


            var result = await _userManager.CreateAsync(user, model.Password);


            if (result.Succeeded)
            {
                // إضافة Role للشركة
                await _userManager.AddToRoleAsync(user, SD.Company);


                // إنشاء الشركة
                var newCompany = new DAL.Model.Company
                {
                    Name = model.CompanyName,
                    Email = model.Email,
                    PlanId = 1,
                    IsActive = true,
                    SubscriptionStart = DateTime.Now,
                    SubscriptionEnd = DateTime.Now.AddDays(14)
                };


                // حفظ الشركة في قاعدة البيانات
                await _companyRepo.CreateAsync(newCompany);
                await _companyRepo.SaveChangesAsync();


                // ربط المستخدم بالشركة بعد الحصول على Id
                user.CompanyId = newCompany.Id;

                await _userManager.UpdateAsync(user);
                // إنشاء Token لتأكيد الإيميل
                var Token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // إنشاء رابط التأكيد
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        UserId = user.Id, Token
                    },
                    Request.Scheme
                );

                await _emailSender.SendEmailAsync(
                    user.Email,"Confirm Your Email",$"Please confirm your email by clicking this link: {confirmationLink}"
                );

                TempData["Success CREATED"] ="Account created please confirm your account";
                return RedirectToAction("Index","Home",new {area="Customer"});
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }


            return View(model);
        }
        // ==================== confirm account ====================
        public async Task<IActionResult> ConfirmEmail(string userId, string Token)
        {

            var applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser == null)
            {
                return RedirectToAction("NotFoundPage", "Home", new { area = "Customer" });
            }

            var result = await _userManager.ConfirmEmailAsync(applicationUser, Token);

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
        // ==================== Login (get) ====================
        public async Task<IActionResult> Login()
        {
            
            return View();
        }

        // ==================== Login (POST) ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. التحقق من وجود اليوزر
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
                return View(model);
            }

            // 2. التحقق إن الحساب مش Locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "تم تعليق حسابك مؤقتاً بسبب محاولات تسجيل دخول متكررة. حاول بعد قليل.");
                return View(model);
            }

            // 4. تسجيل الدخول
            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true  
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectBasedOnRole();
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            return View(model);
        }
        

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid password reset link.");
            }

            return View(new ResetPasswordVm
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return View("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
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
        // ==================== Logout ====================

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }


    }
}
