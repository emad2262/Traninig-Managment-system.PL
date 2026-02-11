

using Microsoft.EntityFrameworkCore;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Courses>,ICourseRepo
    {
        private readonly ApplicationDbContext _Context;

        public CoursesRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            _Context = applicationDbContext;
        }
        public async Task<IEnumerable<Courses>> GetCourseByCategoryIdAsync(int categoryId)
        {
            return await GetAllAsync(
                c => c.CategoryId == categoryId,
                c => c.Instructor   
            );
        }
        public async Task<Courses?> GetCourseWithLessonsAsync(int courseId)
        {
            return await _Context.courses
                .Include(c => c.Lessons) // تأكد من عمل Include للدروس
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

    }
}
