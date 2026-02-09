
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

        public async Task<CompanyOverviewVm> GetCompanyOverviewAsync(int companyId)
        {
            // =========================
            // 1️⃣ Get Company
            // =========================
            var company = await _context.companies
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                throw new Exception("Company not found");

            var now = DateTime.UtcNow;

            // =========================
            // 2️⃣ Subscription Dates (SAFE)
            // =========================
            // SubscriptionStart & SubscriptionEnd نوعهم DateTime (مش nullable)
            var subscriptionStart = company.SubscriptionStart;
            var subscriptionEnd = company.SubscriptionEnd;

            var daysRemaining = (subscriptionEnd.Date - now.Date).Days;
            if (daysRemaining < 0)
                daysRemaining = 0;

            // =========================
            // 3️⃣ Plan Name (SAFE)
            // =========================
            var planName =
                company.Plan != null && !string.IsNullOrWhiteSpace(company.Plan.Name)
                    ? company.Plan.Name
                    : "Standard";

            // =========================
            // 4️⃣ Dashboard Metrics
            // =========================

            // Total Courses (Company -> Category -> Course)
            var totalCourses = await _context.courses
                .CountAsync(c =>
                    c.Category != null &&
                    c.Category.CompanyId == companyId);

            // Active Instructors
            var activeInstructors = await _context.instructors
                .CountAsync(i =>
                    i.CompanyId == companyId &&
                    i.IsActive);

            // Total Employees
            var totalEmployees = await _context.employees
                .CountAsync(e =>
                    e.CompanyId == companyId);

            // =========================
            // 5️⃣ Completion Rate (SAFE)
            // =========================
            double completionRate = 0;

            var totalEnrollments = await _context.EmployeeCourses
                .CountAsync(ec =>
                    ec.Course != null &&
                    ec.Course.Category != null &&
                    ec.Course.Category.CompanyId == companyId);

            if (totalEnrollments > 0)
            {
                var completedEnrollments = await _context.EmployeeCourses
                    .CountAsync(ec =>
                        ec.Course != null &&
                        ec.Course.Category != null &&
                        ec.Course.Category.CompanyId == companyId &&
                        ec.IsCompleted);

                completionRate = (completedEnrollments * 100.0) / totalEnrollments;
            }

            // =========================
            // 6️⃣ Build ViewModel
            // =========================
            var vm = new CompanyOverviewVm
            {
                // Plan
                PlanName = planName,

                // Timer
                RegistrationDate = subscriptionStart,
                ExpirationDate = subscriptionEnd,
                DaysRemaining = daysRemaining,

                // Stats
                TotalCourses = totalCourses,
                ActiveInstructors = activeInstructors,
                TotalStudents = totalEmployees,
                CompletionRate = Math.Round(completionRate, 1)
            };

            return vm;
        }

    }
}
