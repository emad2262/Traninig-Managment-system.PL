namespace Traninig_Managment_system.DAL.Repo
{
    public class CompanyRepo : Repo<Company>, ICompanyRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public CompanyRepo(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DateTime?> GetCompanyExpirationDateAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.companies
                .Where(c => c.Id == companyId)
                .Select(c => (DateTime?)c.SubscriptionEnd)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Employee>> GetTopPerformersAsync(int companyId, int count, CancellationToken cancellationToken = default)
        {
            return await _dbContext.employees
                .Where(e => e.CompanyId == companyId && e.IsActive)
                .OrderByDescending(e => e.Points)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsEmailTakenAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.companies.Where(c => c.Email == email);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync(cancellationToken);
        }
    }
}
