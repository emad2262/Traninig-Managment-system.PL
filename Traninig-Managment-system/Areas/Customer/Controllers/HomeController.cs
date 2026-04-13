using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        public HomeController()
        {
            
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
