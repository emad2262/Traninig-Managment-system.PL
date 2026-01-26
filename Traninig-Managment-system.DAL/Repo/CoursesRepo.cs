

namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Courses>
    {
        public CoursesRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
