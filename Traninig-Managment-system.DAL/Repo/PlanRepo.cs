namespace Traninig_Managment_system.DAL.Repo
{
    public class PlanRepo : Repo<Plan>, IPlanRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public PlanRepo(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.plans
                .Include(p => p.Features)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);
        }
    }
}
