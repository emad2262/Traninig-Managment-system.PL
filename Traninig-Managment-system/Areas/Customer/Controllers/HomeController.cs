using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Traninig_Managment_system.BLL.Services.Interfaces;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IMainPage _mainPage;

        public HomeController(IMainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _mainPage.GetPlansAsync();
            var companies =  await _mainPage.GetCompaniesAsync();
            MainPageVm mainPageVm = new MainPageVm
            {
                Plans = plans,
                Companies = companies
            };
            return View(mainPageVm);
        }
    }
}
