using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Employee.Controllers
{
    public class HomeController : Controller
    {
        [Area("Employee")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
