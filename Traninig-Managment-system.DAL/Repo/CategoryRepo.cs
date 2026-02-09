

namespace Traninig_Managment_system.DAL.Repo
{
    public class CategoryRepo : Repo<CourseCategory>, ICategoryRepo
    {
        private readonly ApplicationDbContext _Context;

        public CategoryRepo(ApplicationDbContext applicationDbContext): base(applicationDbContext)
        {
            _Context = applicationDbContext;
        }


        public async Task<IEnumerable<CourseCategory>> GetAllCategory(int companyid)
        {
            return await _Context.CourseCategories
                .Where(e => e.CompanyId == companyid)
                .Include(e => e.Courses)
                .ThenInclude(e => e.Instructor).ToListAsync();
                
        }

        public Task AddCourseCategory(int CompanyId)
        {
            throw new NotImplementedException();
        }
    }
}
