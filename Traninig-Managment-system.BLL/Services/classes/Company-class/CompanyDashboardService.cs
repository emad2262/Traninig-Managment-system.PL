using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class CompanyDashboardService : ICompanyDashboardService
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly IEmployeeCertificateRepo _certificateRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IInstructorRepo _instructorRepo;
        private readonly IEmployeeCourseRepo _employeeCourseRepo;
        private readonly IEmployeeBadgeRepo _employeeBadgeRepo;

        public CompanyDashboardService(
            ICompanyRepo companyRepo,
            ICourseRepo courseRepo,
            IEmployeeCertificateRepo certificateRepo,
            IEmployeeRepo employeeRepo,
            IInstructorRepo instructorRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            IEmployeeBadgeRepo employeeBadgeRepo)
        {
            _companyRepo = companyRepo;
            _courseRepo = courseRepo;
            _certificateRepo = certificateRepo;
            _employeeRepo = employeeRepo;
            _instructorRepo = instructorRepo;
            _employeeCourseRepo = employeeCourseRepo;
            _employeeBadgeRepo = employeeBadgeRepo;
        }

        public async Task<CompanyOverviewVm> GetDashboardDataAsync(int companyId)
        {
            var vm = new CompanyOverviewVm
            {
                ExpirationDate = await _companyRepo.GetCompanyExpirationDateAsync(companyId) ?? DateTime.MinValue,
                TotalEmployees = await _employeeRepo.CountAsync(e => e.CompanyId == companyId && e.IsActive),
                TotalCourses = await _courseRepo.CountAsync(c => c.Category.CompanyId == companyId && c.IsPublished),
                ActiveInstructors = await _instructorRepo.CountAsync(i => i.CompanyId == companyId && i.IsActive),
                CompletionRate = await ComputeCompletionRateAsync(companyId),
                PendingCertificates = await _certificateRepo.CountPendingAsync(companyId)
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
            var progresses = await _employeeCourseRepo.GetCompanyProgressesAsync(companyId);

            if (progresses.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round(progresses.Average());
        }

        private async Task<List<ActivityTimelineVm>> BuildRecentActivitiesAsync(int companyId, int take)
        {
            var assigned = (await _employeeCourseRepo.GetRecentCompanyAssignmentsAsync(companyId, take))
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
                .ToList();

            var completed = (await _employeeCourseRepo.GetRecentCompanyCompletionsAsync(companyId, take))
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
                .ToList();

            var badges = (await _employeeBadgeRepo.GetRecentCompanyBadgesAsync(companyId, take))
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
                .ToList();

            var newCourses = (await _courseRepo.GetRecentInstructorCoursesAsync(companyId, take))
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
                .ToList();

            var certificates = (await _certificateRepo.GetRecentCompanyCertificatesAsync(companyId, take))
                .Select(c => new ActivityTimelineVm
                {
                    ActivityType = c.Status == CertificateStatus.Issued
                        ? ActivityType.CertificateIssued
                        : ActivityType.CertificateRequested,
                    ActorName = c.Employee?.Name ?? "Employee",
                    ActorRole = "Employee",
                    ActionText = c.Status == CertificateStatus.Issued
                        ? "received certificate for"
                        : "requested certificate for",
                    TargetName = c.Course?.Title ?? "Course",
                    ContextName = c.CertificateNumber,
                    ActionDate = c.Status == CertificateStatus.Issued
                        ? c.IssuedAt ?? c.RequestedAt
                        : c.RequestedAt
                })
                .ToList();

            return assigned
                .Concat(completed)
                .Concat(badges)
                .Concat(newCourses)
                .Concat(certificates)
                .OrderByDescending(a => a.ActionDate)
                .Take(take)
                .ToList();
        }
    }
}
