using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class LessonRepo : Repo<Lesson>, ILessonRepo
    {
        public LessonRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
