

namespace Traninig_Managment_system.DAL.Repo
{
    public class LessonsRepo : Repo<Lesson>
    {
        public LessonsRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
