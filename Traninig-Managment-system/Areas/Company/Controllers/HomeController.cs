using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
