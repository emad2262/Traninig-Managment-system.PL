using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class ExamRepo : Repo<Exam>, IExamRepo
    {
        public ExamRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
