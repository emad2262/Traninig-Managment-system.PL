using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeRepo _employeeRepo;

        public EmployeeManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            IEmployeeRepo employeeRepo)
        {
            _userManager = userManager;
            _context = context;
            _employeeRepo = employeeRepo;
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
    }
}