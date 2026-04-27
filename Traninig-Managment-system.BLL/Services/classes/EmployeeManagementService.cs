using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly IEmployeeCourseRepo _employeeCourseRepo;
        private readonly ICategoryRepo _categoryRepo;

        public EmployeeManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            IEmployeeRepo employeeRepo,
            ICourseRepo courseRepo,
            IEmployeeCourseRepo employeeCourseRepo,
            ICategoryRepo categoryRepo)
        {
            _userManager = userManager;
            _context = context;
            _employeeRepo = employeeRepo;
            _courseRepo = courseRepo;
            _employeeCourseRepo = employeeCourseRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<ServiceResult<int>> AddEmployeeAsync(AddEmployeeVm model, int companyId)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return new ServiceResult<int>
                {
                    Message = "Email already exists.",
                    IsSuccess = false,

                };
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CompanyId = companyId
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return new ServiceResult<int>
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    IsSuccess = false,

                };
            await _userManager.AddToRoleAsync(user, SD.Employee);
            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                JobTitle = model.JobTitle,
                IsActive = model.IsActive,
                Points = 0,
                CompanyId = companyId,
                UserId = user.Id
            };
            try
            {
                await _employeeRepo.CreateAsync(employee);
                return new ServiceResult<int>
                {
                    Data = employee.Id,
                    Message = "Employee added successfully.",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return new ServiceResult<int>
                {
                    Message = "Failed to add employee: " + ex.Message,
                    IsSuccess = false,

                };
            }
        }


        public async Task<IEnumerable<ListEmployeeVm>> GetEmployeesWithCoursesCountAsync(int companyId)
        {
            var employees = await _employeeRepo.GetAllAsync(
                e => e.CompanyId == companyId,
                e => e.EmployeeCourses
            );

            return employees.Select(e => new ListEmployeeVm
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                JobTitle = e.JobTitle,
                IsActive = e.IsActive,
                Points = e.Points,
                CoursesCount = e.EmployeeCourses.Count()
            }).ToList();
        }
        public async Task<EmployeeDetailsVm> GetEmployeeByIdAsync(int companyId, int employeeId)
        {
            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);

            if (employee == null)
                return null;

            var employeeDetails = new EmployeeDetailsVm
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive,
                Points = employee.Points,

                courses = employee.EmployeeCourses.Select(ec => new EmployeeCourseVm
                {
                    CourseId = ec.CourseId,
                    CourseName = ec.Course?.Title ?? "",
                    InstructorName = ec.Course?.Instructor?.FullName ?? "",
                    Status = ec.Status,
                    Progress = ec.Progress,
                    FinalScore = ec.FinalScore,
                    AssignedAt = ec.AssignedAt,
                    CompletedAt = ec.CompletedAt
                }).ToList()
            };

            return employeeDetails;
        }

        public async Task<AssignCourseVm?> GetAssignCourseDataAsync(int companyId, int employeeId, string? search = null, int? categoryId = null)
        {
            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);
            if (employee == null)
                return null;

            var assignedIds = employee.EmployeeCourses.Select(ec => ec.CourseId).ToHashSet();

            var courses = await _courseRepo.GetAllAsync(
                c => c.Category.CompanyId == companyId && c.IsPublished,
                c => c.Category,
                c => c.Instructor!
            );

            var available = courses.Where(c => !assignedIds.Contains(c.Id));

            if (categoryId.HasValue && categoryId.Value > 0)
                available = available.Where(c => c.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                available = available.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

            var categories = await _categoryRepo.GetAllAsync(c => c.CompanyId == companyId);

            return new AssignCourseVm
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                EmployeeEmail = employee.Email,
                JobTitle = employee.JobTitle,
                Search = search ?? string.Empty,
                CategoryId = categoryId,
                Categories = categories.Select(c => new CategoryDisplayVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    CompanyId = c.CompanyId
                }).ToList(),
                AvailableCourses = available.Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Logo = c.logo,
                    DurationInHours = c.DurationInHours,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsPublished = c.IsPublished,
                    CategoryId = c.CategoryId,
                    CategoryName = c.Category?.Name,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor?.FullName
                }).ToList(),
                AssignedCourses = employee.EmployeeCourses.Select(ec => new EmployeeCourseVm
                {
                    CourseId = ec.CourseId,
                    CourseName = ec.Course?.Title ?? string.Empty,
                    InstructorName = ec.Course?.Instructor?.FullName ?? string.Empty,
                    Status = ec.Status,
                    Progress = ec.Progress,
                    AssignedAt = ec.AssignedAt
                }).ToList()
            };
        }

        public async Task<ServiceResult<int>> AssignCoursesToEmployeeAsync(int companyId, int employeeId, IEnumerable<int> courseIds)
        {
            if (courseIds == null || !courseIds.Any())
                return new ServiceResult<int> { IsSuccess = false, Message = "No courses selected." };

            var employee = await _employeeRepo.GetEmployeeWithCoursesAsync(companyId, employeeId);
            if (employee == null)
                return new ServiceResult<int> { IsSuccess = false, Message = "Employee not found." };

            var alreadyAssigned = employee.EmployeeCourses.Select(ec => ec.CourseId).ToHashSet();

            var validCourses = await _courseRepo.GetAllAsync(
                c => courseIds.Contains(c.Id) && c.Category.CompanyId == companyId && c.IsPublished
            );

            var toAssign = validCourses
                .Where(c => !alreadyAssigned.Contains(c.Id))
                .ToList();

            if (!toAssign.Any())
                return new ServiceResult<int> { IsSuccess = false, Message = "Selected courses are not available or already assigned." };

            var added = 0;
            foreach (var course in toAssign)
            {
                var record = new EmployeeCourse
                {
                    EmployeeId = employeeId,
                    CourseId = course.Id,
                    AssignedAt = DateTime.UtcNow,
                    Status = CourseStatus.NotStarted,
                    Progress = 0
                };

                if (await _employeeCourseRepo.CreateAsync(record))
                    added++;
            }

            if (added == 0)
                return new ServiceResult<int> { IsSuccess = false, Message = "Failed to assign courses." };

            return new ServiceResult<int>
            {
                IsSuccess = true,
                Data = added,
                Message = added == 1
                    ? "Course assigned successfully."
                    : $"{added} courses assigned successfully."
            };
        }
    }
}