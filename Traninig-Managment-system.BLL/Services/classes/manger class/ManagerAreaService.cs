using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class ManagerAreaService : IManagerAreaService
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly IPlanRepo _planRepo;
        private readonly IPlanFeatureRepo _planFeatureRepo;
        private readonly ICompanyNotificationRepo _companyNotificationRepo;
        private readonly IEmployeeRepo _employeeRepo;

        public ManagerAreaService(
            ICompanyRepo companyRepo,
            IPlanRepo planRepo,
            IPlanFeatureRepo planFeatureRepo,
            ICompanyNotificationRepo companyNotificationRepo,
            IEmployeeRepo employeeRepo)
        {
            _companyRepo = companyRepo;
            _planRepo = planRepo;
            _planFeatureRepo = planFeatureRepo;
            _companyNotificationRepo = companyNotificationRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<ManagerDashboardVm> GetDashboardAsync()
        {
            var companies = (await _companyRepo.GetAllAsync(
                null,
                c => c.Plan,
                c => c.Employees,
                c => c.Instructors,
                c => c.CoursesCategories,
                c => c.Notifications)).ToList();

            var plans = (await _planRepo.GetAllAsync(
                null,
                p => p.Features,
                p => p.Companys)).ToList();

            var employees = (await _employeeRepo.GetAllAsync()).ToList();
            var notifications = await _companyNotificationRepo.CountAsync(n => true);
            var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);

            return new ManagerDashboardVm
            {
                TotalCompanies = companies.Count,
                ActiveCompanies = companies.Count(c => c.IsActive),
                TotalEmployees = employees.Count,
                TotalPlans = plans.Count,
                TotalNotifications = notifications,
                ExpiringSoonCount = companies.Count(c => c.SubscriptionEnd.Date <= DateTime.UtcNow.Date.AddDays(30)),
                CompanyGrowth = BuildMonthlySeries(
                    companies.Where(c => c.SubscriptionStart >= startDate).Select(c => c.SubscriptionStart),
                    startDate),
                EmployeeGrowth = BuildMonthlySeries(
                    employees.Where(e => e.CreatedAt >= startDate).Select(e => e.CreatedAt),
                    startDate),
                ExpiringCompanies = companies
                    .Where(c => c.SubscriptionEnd.Date <= DateTime.UtcNow.Date.AddDays(30))
                    .OrderBy(c => c.SubscriptionEnd)
                    .Take(5)
                    .Select(MapCompany)
                    .ToList(),
                RecentCompanies = companies
                    .OrderByDescending(c => c.SubscriptionStart)
                    .Take(5)
                    .Select(MapCompany)
                    .ToList(),
                Plans = plans.Select(MapPlan).OrderBy(p => p.Type).ToList()
            };
        }

        public async Task<List<ManagerPlanVm>> GetPlansAsync()
        {
            var plans = await _planRepo.GetAllAsync(
                null,
                p => p.Features,
                p => p.Companys);

            return plans
                .Select(MapPlan)
                .OrderBy(p => p.Type)
                .ThenBy(p => p.Price)
                .ToList();
        }

        public async Task<ManagerPlanVm?> GetPlanAsync(int id)
        {
            var plan = await _planRepo.GetOneAsync(
                p => p.Id == id,
                p => p.Features,
                p => p.Companys);

            return plan == null ? null : MapPlan(plan);
        }

        public async Task<ServiceResult<int>> CreatePlanAsync(ManagerPlanVm model)
        {
            var validation = ValidatePlan(model);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = validation.Message };
            }

            var plan = new Plan
            {
                Name = model.Name.Trim(),
                Type = model.Type,
                Price = model.Price,
                DurationInDays = model.DurationInDays,
                MaxEmployees = model.MaxEmployees,
                MaxCourses = model.MaxCourses,
                IsActive = model.IsActive
            };

            var created = await _planRepo.CreateAsync(plan);
            if (!created)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Failed to create the plan." };
            }

            var features = ParseFeatures(model.FeaturesText, plan.Id);
            foreach (var feature in features)
            {
                await _planFeatureRepo.CreateAsync(feature);
            }

            return new ServiceResult<int>
            {
                IsSuccess = true,
                Data = plan.Id,
                Message = "Plan created successfully."
            };
        }

        public async Task<ServiceResult<bool>> UpdatePlanAsync(ManagerPlanVm model)
        {
            var validation = ValidatePlan(model);
            if (!validation.IsSuccess)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = validation.Message };
            }

            var currentPlan = await _planRepo.GetOneAsync(
                p => p.Id == model.Id,
                p => p.Features,
                p => p.Companys);

            if (currentPlan == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Plan not found." };
            }

            currentPlan.Name = model.Name.Trim();
            currentPlan.Type = model.Type;
            currentPlan.Price = model.Price;
            currentPlan.DurationInDays = model.DurationInDays;
            currentPlan.MaxEmployees = model.MaxEmployees;
            currentPlan.MaxCourses = model.MaxCourses;
            currentPlan.IsActive = model.IsActive;

            var updated = await _planRepo.UpdateAsync(currentPlan);
            if (!updated)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Failed to update the plan." };
            }

            foreach (var feature in currentPlan.Features.ToList())
            {
                await _planFeatureRepo.Delete(feature);
            }

            var newFeatures = ParseFeatures(model.FeaturesText, model.Id);
            foreach (var feature in newFeatures)
            {
                await _planFeatureRepo.CreateAsync(feature);
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "Plan updated successfully."
            };
        }

        public async Task<ServiceResult<bool>> DeletePlanAsync(int id)
        {
            var plan = await _planRepo.GetOneAsync(
                p => p.Id == id,
                p => p.Features,
                p => p.Companys);

            if (plan == null)
            {
                return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = "Plan not found." };
            }

            if (plan.Companys.Any())
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "This plan is assigned to companies and cannot be deleted."
                };
            }

            foreach (var feature in plan.Features.ToList())
            {
                await _planFeatureRepo.Delete(feature);
            }

            var deleted = await _planRepo.Delete(plan);
            return new ServiceResult<bool>
            {
                IsSuccess = deleted,
                Data = deleted,
                Message = deleted ? "Plan deleted successfully." : "Plan could not be deleted."
            };
        }

        public async Task<List<ManagerCompanyVm>> GetCompaniesAsync(string? search = null)
        {
            var companies = (await _companyRepo.GetAllAsync(
                c => string.IsNullOrWhiteSpace(search) ||
                     c.Name.Contains(search) ||
                     c.Email.Contains(search),
                c => c.Plan,
                c => c.Employees,
                c => c.Instructors,
                c => c.CoursesCategories,
                c => c.Notifications)).ToList();

            return companies
                .Select(MapCompany)
                .OrderBy(c => c.DaysToRenewal)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public async Task<ManagerCompanyDetailsVm?> GetCompanyDetailsAsync(int id)
        {
            var company = await _companyRepo.GetOneAsync(
                c => c.Id == id,
                c => c.Plan,
                c => c.Employees,
                c => c.Instructors,
                c => c.CoursesCategories,
                c => c.Notifications);

            if (company == null)
            {
                return null;
            }

            var planWithFeatures = await _planRepo.GetOneAsync(
                p => p.Id == company.PlanId,
                p => p.Features);

            return new ManagerCompanyDetailsVm
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Logo = company.Logo,
                PlanName = company.Plan?.Name ?? "No Plan",
                IsActive = company.IsActive,
                EmployeeCount = company.Employees.Count,
                InstructorCount = company.Instructors.Count,
                CategoryCount = company.CoursesCategories.Count,
                NotificationCount = company.Notifications.Count,
                ExpiringSoon = company.SubscriptionEnd.Date <= DateTime.UtcNow.Date.AddDays(30),
                DaysToRenewal = Math.Max(0, (company.SubscriptionEnd.Date - DateTime.UtcNow.Date).Days),
                SubscriptionStart = company.SubscriptionStart,
                SubscriptionEnd = company.SubscriptionEnd,
                PlanFeatures = planWithFeatures?.Features
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new PlanFeatureVm
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Description = f.Description,
                        IsHighlighted = f.IsHighlighted,
                        SortOrder = f.SortOrder
                    }).ToList() ?? new List<PlanFeatureVm>(),
                Notifications = company.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new ManagerNotificationVm
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        DeliveryChannel = n.DeliveryChannel,
                        IsSent = n.IsSent,
                        CreatedAt = n.CreatedAt,
                        SentAt = n.SentAt
                    }).ToList(),
                NotificationForm = new CreateCompanyNotificationVm
                {
                    CompanyId = company.Id,
                    Title = $"Renewal reminder for {company.Name}",
                    Message = $"Hello {company.Name}, your subscription will expire on {company.SubscriptionEnd:dd MMM yyyy}. Please renew to keep your workspace active.",
                    Type = CompanyNotificationType.RenewalReminder
                }
            };
        }

        public async Task<ServiceResult<int>> SendNotificationAsync(CreateCompanyNotificationVm model)
        {
            if (model.CompanyId <= 0)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "A valid company is required." };
            }

            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Message))
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Title and message are required." };
            }

            var company = await _companyRepo.GetOneAsync(c => c.Id == model.CompanyId);
            if (company == null)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Company not found." };
            }

            var notification = new CompanyNotification
            {
                CompanyId = model.CompanyId,
                Title = model.Title.Trim(),
                Message = model.Message.Trim(),
                Type = model.Type,
                DeliveryChannel = string.IsNullOrWhiteSpace(model.DeliveryChannel) ? "Dashboard" : model.DeliveryChannel.Trim(),
                IsSent = true,
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            };

            var created = await _companyNotificationRepo.CreateAsync(notification);
            return new ServiceResult<int>
            {
                IsSuccess = created,
                Data = notification.Id,
                Message = created ? "Notification sent successfully." : "Failed to send the notification."
            };
        }

        public async Task<ServiceResult<int>> SendRenewalReminderAsync(int companyId)
        {
            var company = await _companyRepo.GetOneAsync(c => c.Id == companyId);
            if (company == null)
            {
                return new ServiceResult<int> { IsSuccess = false, Message = "Company not found." };
            }

            return await SendNotificationAsync(new CreateCompanyNotificationVm
            {
                CompanyId = companyId,
                Title = $"Renewal reminder for {company.Name}",
                Message = $"Hello {company.Name}, your subscription is scheduled to expire on {company.SubscriptionEnd:dd MMM yyyy}. Please renew your plan to avoid interruption.",
                Type = CompanyNotificationType.RenewalReminder,
                DeliveryChannel = "Dashboard"
            });
        }

        private static ServiceResult<bool> ValidatePlan(ManagerPlanVm model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new ServiceResult<bool> { IsSuccess = false, Message = "Plan name is required." };
            }

            if (model.Price < 0 || model.DurationInDays <= 0 || model.MaxEmployees <= 0 || model.MaxCourses <= 0)
            {
                return new ServiceResult<bool> { IsSuccess = false, Message = "Plan limits and duration must be greater than zero." };
            }

            return new ServiceResult<bool> { IsSuccess = true, Data = true };
        }

        private static List<ManagerChartPointVm> BuildMonthlySeries(IEnumerable<DateTime> dates, DateTime startDate)
        {
            var normalizedStart = new DateTime(startDate.Year, startDate.Month, 1);
            var buckets = dates
                .GroupBy(d => new DateTime(d.Year, d.Month, 1))
                .ToDictionary(g => g.Key, g => g.Count());

            var points = new List<ManagerChartPointVm>();
            for (var i = 0; i < 6; i++)
            {
                var month = normalizedStart.AddMonths(i);
                buckets.TryGetValue(month, out var value);
                points.Add(new ManagerChartPointVm
                {
                    Label = month.ToString("MMM"),
                    Value = value
                });
            }

            return points;
        }

        private static ManagerPlanVm MapPlan(Plan plan)
        {
            var orderedFeatures = plan.Features
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Name)
                .Select(f => new PlanFeatureVm
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    IsHighlighted = f.IsHighlighted,
                    SortOrder = f.SortOrder
                }).ToList();

            return new ManagerPlanVm
            {
                Id = plan.Id,
                Name = plan.Name,
                Type = plan.Type,
                Price = plan.Price,
                DurationInDays = plan.DurationInDays,
                MaxEmployees = plan.MaxEmployees,
                MaxCourses = plan.MaxCourses,
                IsActive = plan.IsActive,
                CompanyCount = plan.Companys.Count,
                Features = orderedFeatures,
                FeaturesText = string.Join(Environment.NewLine,
                    orderedFeatures.Select(f => string.IsNullOrWhiteSpace(f.Description)
                        ? f.Name
                        : $"{f.Name} | {f.Description}"))
            };
        }

        private static ManagerCompanyVm MapCompany(Company company)
        {
            var daysToRenewal = (company.SubscriptionEnd.Date - DateTime.UtcNow.Date).Days;

            return new ManagerCompanyVm
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Logo = company.Logo,
                PlanName = company.Plan?.Name ?? "No Plan",
                IsActive = company.IsActive,
                EmployeeCount = company.Employees.Count,
                InstructorCount = company.Instructors.Count,
                CategoryCount = company.CoursesCategories.Count,
                NotificationCount = company.Notifications.Count,
                ExpiringSoon = company.SubscriptionEnd.Date <= DateTime.UtcNow.Date.AddDays(30),
                DaysToRenewal = daysToRenewal,
                SubscriptionStart = company.SubscriptionStart,
                SubscriptionEnd = company.SubscriptionEnd
            };
        }

        private static List<PlanFeature> ParseFeatures(string featuresText, int planId)
        {
            if (string.IsNullOrWhiteSpace(featuresText))
            {
                return new List<PlanFeature>();
            }

            return featuresText
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, index) =>
                {
                    var parts = line.Split('|', 2, StringSplitOptions.TrimEntries);
                    return new PlanFeature
                    {
                        PlanId = planId,
                        Name = parts[0],
                        Description = parts.Length > 1 ? parts[1] : string.Empty,
                        IsHighlighted = index < 2,
                        SortOrder = index + 1
                    };
                })
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .ToList();
        }
    }
}
