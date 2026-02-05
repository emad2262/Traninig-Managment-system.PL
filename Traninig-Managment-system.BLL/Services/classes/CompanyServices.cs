
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.DAL.Data;
using Traninig_Managment_system.DAL.Repo;

namespace Traninig_Managment_system.BLL.Services.classes
{
   
    public class CompanyServices : ICompanyServices
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly ApplicationDbContext _context;

        public CompanyServices(ICompanyRepo companyRepo ,ApplicationDbContext applicationDbContext)
        {
           _companyRepo = companyRepo;
            _context = applicationDbContext;
        }

        public async Task<Company> CreateAsync(Company company)
        {
            // 1️⃣ هات Plan افتراضي
            var defaultPlan = await _context.plans
                .FirstOrDefaultAsync(p => p.IsActive);

            if (defaultPlan == null)
                throw new Exception("No active plan found");

            // 2️⃣ اربط الشركة بالـ Plan
            company.PlanId = defaultPlan.Id;

            // 3️⃣ حدد نهاية الاشتراك
            company.SubscriptionEnd = company.SubscriptionStart.AddDays(
                defaultPlan.DurationInDays
            );

            // 4️⃣ احفظ
            _context.companies.Add(company);
            await _context.SaveChangesAsync();

            return company;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _companyRepo.GetOneAsync(e=>e.Id==id);
        }
    }
}
