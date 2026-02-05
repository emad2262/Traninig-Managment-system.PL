using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    public class CoursesController : Controller
    {
        [Area("Company")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
