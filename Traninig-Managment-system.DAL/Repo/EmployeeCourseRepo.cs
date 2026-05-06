

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeCourseRepo : Repo<EmployeeCourse>, IEmployeeCourseRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeCourseRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<double>> GetCompanyProgressesAsync(int companyId)
        {
            return await _context.EmployeeCourses
                .AsNoTracking()
                .Where(ec => ec.Employee.CompanyId == companyId)
                .Select(ec => ec.Progress)
                .ToListAsync();
        }

        public async Task<List<EmployeeCourse>> GetRecentCompanyAssignmentsAsync(int companyId, int take)
        {
            return await CompanyEmployeeCourseQuery(companyId)
                .OrderByDescending(ec => ec.AssignedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<EmployeeCourse>> GetRecentCompanyCompletionsAsync(int companyId, int take)
        {
            return await CompanyEmployeeCourseQuery(companyId)
                .Where(ec => ec.CompletedAt != null)
                .OrderByDescending(ec => ec.CompletedAt)
                .Take(take)
                .ToListAsync();
        }

        private IQueryable<EmployeeCourse> CompanyEmployeeCourseQuery(int companyId)
        {
            return _context.EmployeeCourses
                .AsNoTracking()
                .Include(ec => ec.Employee)
                .Include(ec => ec.Course)
                    .ThenInclude(c => c.Category)
                .Where(ec => ec.Employee.CompanyId == companyId);
        }
    }
}
