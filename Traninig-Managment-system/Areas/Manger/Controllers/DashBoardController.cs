using Microsoft.AspNetCore.Mvc;

namespace Traninig_Managment_system.Areas.Manger.Controllers
{
    [Area("Manger")]
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
