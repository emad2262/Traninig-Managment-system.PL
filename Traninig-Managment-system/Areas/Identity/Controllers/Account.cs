using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.ModelVm;
using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.Utality;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Traninig_Managment_system.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller // Inherit from Controller to use View()
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm registerVm)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVm);
            }
            ApplicationUser applicationUser = new ApplicationUser()
            {
                UserName = registerVm.Name,
                Email = registerVm.Email,

            };
            var result = await userManager.CreateAsync(applicationUser,registerVm.passwood);
            if (result.Succeeded)
            {
                ///generate token
                var tokeen =await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                //generate url 
                var link = Url.Action("ConfirmEmail", "Account", new
                {
                    area = "Identity",
                    UserId = applicationUser.Id,
                    tokeen
                }, protocol: Request.Scheme
                );
                // confirm message
               await _emailSender.SendEmailAsync(registerVm.Email, "ConfirmEmail",
                    $"please confirm your email by click here <a href='{link}'>confirm</a>");


                return RedirectToAction("Index", "Home", new {area="Customer" });
            }
            else
            {
                foreach(var errors in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, errors.Description);

                }
            }
            return View(registerVm);
        }
        //confirm account 
        public async Task<IActionResult> ConfirmEmail(string UserId, string tokeen)
        {

            var applicationUser = await userManager.FindByIdAsync(UserId);

            if (applicationUser == null)
            {
                return RedirectToAction("NotFoundPage", "Home", new { area = "Customer" });
            }

            var result = await userManager.ConfirmEmailAsync(applicationUser, tokeen);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        // login 
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }
            var applicationuser =await userManager.FindByEmailAsync(loginVm.Email);
            if(applicationuser is not null)
            {
                var cheakpassword = await userManager.CheckPasswordAsync(applicationuser, loginVm.Password);
                if (cheakpassword )
                {
                    await signInManager.SignInAsync(applicationuser, loginVm.RememberMe);
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                else
                {
                    
                    ModelState.AddModelError(string.Empty,"errorMessage");

                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");

            }
            return View(loginVm);

        }
        ///log out
        ///
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();

            TempData["notification"] = "log out";
            return RedirectToAction("Index", "Home", new { Area = "Customar" });
        }
    }
}
