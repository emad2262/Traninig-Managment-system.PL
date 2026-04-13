using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class InstructorRepo : Repo<Instructor>, IInstructorRepo
    {
        public InstructorRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
