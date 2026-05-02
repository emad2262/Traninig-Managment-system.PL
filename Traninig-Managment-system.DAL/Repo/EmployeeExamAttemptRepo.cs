using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeExamAttemptRepo : Repo<EmployeeExamAttempt>, IEmployeeExamAttemptRepo
    {
        public EmployeeExamAttemptRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
