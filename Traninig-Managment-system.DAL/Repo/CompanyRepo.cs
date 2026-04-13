

namespace Traninig_Managment_system.DAL.Repo
{
    public class CompanyRepo : Repo<Company>, ICompanyRepo
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<DateTime?> GetCompanyExpirationDateAsync(int companyId)
        {
            return await _context.companies
                .Where(c => c.Id == companyId)
                .Select(c => c.SubscriptionEnd)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Employee>> GetTopPerformersAsync(int companyId, int take)
        {
            return await _context.employees
                .AsNoTracking()
                .Where(e => e.CompanyId == companyId)
                .OrderByDescending(e => e.Points) 
                .Take(take)
                .ToListAsync();
        }

   
    }
}
