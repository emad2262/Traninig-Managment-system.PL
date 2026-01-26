

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class PlanServices : IPlanService
    {
        private readonly IplanRepo _planRepo;

        public PlanServices(IplanRepo planRepo)
        {
            _planRepo = planRepo;
        }

        public async Task<bool> AddAsync(Plan plan)
        {
            if (plan is null)
                throw new ArgumentNullException(nameof(plan), "Plan cannot be null");

            return await _planRepo.CreateAsync(plan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _planRepo.GetOneAsync(p => p.Id == id);

            if (plan is null)
                throw new ArgumentException($"No plan found with ID {id}", nameof(id));

            return await _planRepo.Delete(plan);
        }

        public async Task<IEnumerable<Plan>> GetAllAsync()
        {
            return await _planRepo.GetAllAsync();
        }

        public async Task<Plan?> GetByIdAsync(int id)
        {
            return await _planRepo.GetOneAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateAsync(Plan plan)
        {
            var existingPlan = await _planRepo.GetOneAsync(p => p.Id == plan.Id);

            if (existingPlan is null)
                throw new ArgumentException($"No plan found with ID {plan.Id}", nameof(plan));

            return await _planRepo.UdateAsync(plan);
        }
    }
}