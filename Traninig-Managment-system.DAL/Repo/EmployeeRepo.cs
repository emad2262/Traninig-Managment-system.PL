

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeRepo : Repo<Employee>, IEmployeeRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeWithCoursesAsync(int companyId, int employeeId)
        {
            return await _context.employees
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId && e.Id == employeeId)
                .Include(e => e.EmployeeCourses)
                    .ThenInclude(ec => ec.Course)
                        .ThenInclude(c => c.Category)
                .Include(e => e.EmployeeCourses)
                    .ThenInclude(ec => ec.Course)
                        .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync();
        }
    }
}
