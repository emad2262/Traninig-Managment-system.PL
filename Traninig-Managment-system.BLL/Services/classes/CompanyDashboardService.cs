using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class CompanyDashboardService : ICompanyDashboardService
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly ApplicationDbContext _context;

        public CompanyDashboardService(ICompanyRepo companyRepo, ICourseRepo courseRepo, ApplicationDbContext context)
        {
            _companyRepo = companyRepo;
            _courseRepo = courseRepo;
            _context = context;
        }

        public async Task<CompanyOverviewVm> GetDashboardDataAsync(int companyId)
        {
            var vm = new CompanyOverviewVm
            {
                ExpirationDate = await _companyRepo.GetCompanyExpirationDateAsync(companyId) ?? DateTime.MinValue,
                TotalEmployees = await _context.employees.CountAsync(e => e.CompanyId == companyId && e.IsActive),
                TotalCourses = await _courseRepo.CountAsync(c => c.Category.CompanyId == companyId && c.IsPublished),
                ActiveInstructors = await _context.instructors.CountAsync(i => i.CompanyId == companyId && i.IsActive),
                CompletionRate = await ComputeCompletionRateAsync(companyId)
            };

            var topEmployees = await _companyRepo.GetTopPerformersAsync(companyId, 3);
            int rank = 1;

            foreach (var emp in topEmployees)
            {
                vm.TopPerformers.Add(new TopEmployeeVm
                {
                    Rank = rank++,
                    EmployeeName = emp.Name,
                    JobTitle = emp.JobTitle ?? "Employee",
                    Points = emp.Points
                });
            }

            vm.RecentActivities = await BuildRecentActivitiesAsync(companyId, 12);

            return vm;
        }

        private async Task<int> ComputeCompletionRateAsync(int companyId)
        {
            var progresses = await _context.EmployeeCourses
                .AsNoTracking()
                .Where(ec => ec.Employee.CompanyId == companyId)
                .Select(ec => ec.Progress)
                .ToListAsync();

            if (progresses.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round(progresses.Average());
        }

        private async Task<List<ActivityTimelineVm>> BuildRecentActivitiesAsync(int companyId, int take)
        {
            var assigned = await _context.EmployeeCourses
                .AsNoTracking()
                .Where(ec => ec.Employee.CompanyId == companyId)
                .OrderByDescending(ec => ec.AssignedAt)
                .Take(take)
                .Select(ec => new ActivityTimelineVm
                {
                    ActivityType = ActivityType.CourseAssigned,
                    ActorName = ec.Employee.Name,
                    ActorRole = "Employee",
                    ActionText = "started",
                    TargetName = ec.Course.Title,
                    ContextName = ec.Course.Category.Name,
                    ActionDate = ec.AssignedAt
                })
                .ToListAsync();

            var completed = await _context.EmployeeCourses
                .AsNoTracking()
                .Where(ec => ec.Employee.CompanyId == companyId && ec.CompletedAt != null)
                .OrderByDescending(ec => ec.CompletedAt)
                .Take(take)
                .Select(ec => new ActivityTimelineVm
                {
                    ActivityType = ActivityType.CourseCompleted,
                    ActorName = ec.Employee.Name,
                    ActorRole = "Employee",
                    ActionText = "completed",
                    TargetName = ec.Course.Title,
                    ContextName = ec.Course.Category.Name,
                    ActionDate = ec.CompletedAt!.Value
                })
                .ToListAsync();

            var badges = await _context.EmployeeBadges
                .AsNoTracking()
                .Where(eb => eb.Employee.CompanyId == companyId)
                .OrderByDescending(eb => eb.EarnedAt)
                .Take(take)
                .Select(eb => new ActivityTimelineVm
                {
                    ActivityType = ActivityType.BadgeEarned,
                    ActorName = eb.Employee.Name,
                    ActorRole = "Employee",
                    ActionText = "earned the",
                    TargetName = eb.Badge.Name,
                    ContextName = eb.Badge.Tier,
                    ActionDate = eb.EarnedAt
                })
                .ToListAsync();

            var newCourses = await _context.courses
                .AsNoTracking()
                .Where(c => c.Category.CompanyId == companyId && c.InstructorId != null)
                .OrderByDescending(c => c.Id)
                .Take(take)
                .Select(c => new ActivityTimelineVm
                {
                    ActivityType = ActivityType.CourseCreated,
                    ActorName = c.Instructor!.FullName,
                    ActorRole = "Instructor",
                    ActionText = "added a new course",
                    TargetName = c.Title,
                    ContextName = c.Category.Name,
                    ActionDate = c.StartDate
                })
                .ToListAsync();

            return assigned
                .Concat(completed)
                .Concat(badges)
                .Concat(newCourses)
                .OrderByDescending(a => a.ActionDate)
                .Take(take)
                .ToList();
        }
    }
}
