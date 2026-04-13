
namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Course>, ICourseRepo
    {
        private readonly ApplicationDbContext _context;

        public CoursesRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
