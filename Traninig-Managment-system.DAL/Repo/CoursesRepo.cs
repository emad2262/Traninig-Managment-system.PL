

namespace Traninig_Managment_system.DAL.Repo
{
    public class CoursesRepo : Repo<Courses>,ICourseRepo
    {
        public CoursesRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            
        }
        public async Task<IEnumerable<Courses>> GetByCategoryIdAsync(int categoryId)
        {
            return await GetAllAsync(
                c => c.CategoryId == categoryId,
                c => c.Category
            );
        }

        public async Task<IEnumerable<Courses>> GetByInstructorIdAsync(int instructorId)
        {
            return await GetAllAsync(
                c => c.InstructorId == instructorId,
                c => c.Category
            );
        }

        public async Task<Courses?> GetByIdWithDetailsAsync(int courseId)
        {
            return await GetOneAsync(
                c => c.Id == courseId,
                c => c.Category,
                c => c.Lessons
            );
        }
    }
}
