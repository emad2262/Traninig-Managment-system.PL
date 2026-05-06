using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeBadgeRepo : Repo<EmployeeBadge>, IEmployeeBadgeRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeBadgeRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<EmployeeBadge>> GetRecentCompanyBadgesAsync(int companyId, int take)
        {
            return await _context.EmployeeBadges
                .AsNoTracking()
                .Include(eb => eb.Employee)
                .Include(eb => eb.Badge)
                .Where(eb => eb.Employee.CompanyId == companyId)
                .OrderByDescending(eb => eb.EarnedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
