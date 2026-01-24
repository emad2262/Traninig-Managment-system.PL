using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
