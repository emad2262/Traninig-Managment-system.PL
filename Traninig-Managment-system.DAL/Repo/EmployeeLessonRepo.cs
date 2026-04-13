using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeLessonRepo : Repo<EmployeeLesson>, IEmployeeLessonRepo
    {
        public EmployeeLessonRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
