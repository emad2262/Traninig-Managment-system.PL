

namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Courses>,ICourseRepo
    {
        public CoursesRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            
        }
        public async Task<IEnumerable<Courses>> GetCourseByCategoryIdAsync(int categoryId)
        {
            return await GetAllAsync(
                c => c.CategoryId == categoryId,
                c => c.Instructor   
            );
        }

    }
}
