using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = SD.Company)]
    public class CourseController : Controller
    {
      

        public CourseController()
        {
            
        }
    }
}
