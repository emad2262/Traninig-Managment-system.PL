using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CourseChapterRepo : Repo<CourseChapter>, ICourseChapterRepo
    {
        public CourseChapterRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
