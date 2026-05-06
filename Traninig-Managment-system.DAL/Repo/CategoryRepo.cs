using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CategoryRepo : Repo<Category>, ICategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetCompanyCategoriesWithCoursesAsync(int companyId)
        {
            return await _context.CourseCategories
                .AsNoTracking()
                .Include(c => c.Courses)
                    .ThenInclude(c => c.Instructor)
                .Where(c => c.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCompanyCategoryWithCoursesAsync(int companyId, int categoryId)
        {
            return await _context.CourseCategories
                .AsNoTracking()
                .Include(c => c.Courses)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == categoryId);
        }

        public async Task<bool> ExistsWithNameAsync(int companyId, int excludedCategoryId, string normalizedName)
        {
            return await _context.CourseCategories
                .AsNoTracking()
                .AnyAsync(c =>
                    c.CompanyId == companyId &&
                    c.Id != excludedCategoryId &&
                    c.Name.ToLower() == normalizedName);
        }
    }
}
