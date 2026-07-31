using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Employee;
using Traninig_Managment_system.BLL.Services.Interfaces;
using Traninig_Managment_system.DAL.Repo;

namespace Traninig_Managment_system.BLL.Services
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeManagementService(IEmployeeRepo employeeRepo, UserManager<ApplicationUser> userManager)
        {
            _employeeRepo = employeeRepo;
            _userManager = userManager;
        }
        public async Task<int> EmployeeCount(int companyId)
        {
            var categorycount = await _employeeRepo.CountAsync(e => e.CompanyId == companyId);
            return categorycount;
        }
        public async Task<ServiceResult<int>> CreateEmployeeAsync(CreateEmployeDto model, int companyId)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new ServiceResult<int>
                {
                    Message = "Email already exists",
                    IsSuccess = false
                };
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CompanyId = companyId
            };


            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new ServiceResult<int>
                    {
                        Message = string.Join(" | ", result.Errors.Select(e => e.Description)),
                        IsSuccess = false
                    };
                }

                var roleResult = await _userManager.AddToRoleAsync(user, SD.Employee);
                if (!roleResult.Succeeded)
                {
                    return new ServiceResult<int>
                    {
                        Message = string.Join(" | ", roleResult.Errors.Select(e => e.Description)),
                        IsSuccess = false
                    };
                }

                var employee = new Employee
                {
                    Name = model.FullName,
                    Email = model.Email,
                    CompanyId = companyId,
                    UserId = user.Id,
                    IsActive = true
                };

                await _employeeRepo.CreateAsync(employee);
                await _employeeRepo.SaveChangesAsync();

                return new ServiceResult<int>
                {
                    Data = employee.Id,
                    Message = "Instructor registered successfully",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<int>
                {
                    Message = $"Instructor registration failed{ex.Message}",
                    IsSuccess = false
                };
            }
        }

        public async Task<IEnumerable<ListEmployeeDto>> GetListEmployees(int companyId)
        {
            var employees = await _employeeRepo.GetAllAsync(
                e => e.CompanyId == companyId);

            return employees.Select(e => new ListEmployeeDto
            {
                //Id = e.Id,
                //Name = e.Name,
                //Email = e.Email,
                //JobTitle = e.JobTitle,
                //IsActive = e.IsActive,
                //Points = e.Points,
                //CoursesCount = e.EmployeeCourses.Count()
            }).ToList();
        }

        public async Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int employeeId, int companyid)
        {
            var employee = await _employeeRepo.GetOneAsync(e => e.Id == employeeId && e.CompanyId == companyid);
            if (employee == null)
            {
                return null;
            }

            return new EmployeeDetailsDto
            {
                //Id = employee.Id,
                //Name = employee.Name,
                //Email = employee.Email,
                //JobTitle = employee.JobTitle,
                //IsActive = employee.IsActive,
                //Points = employee.Points,
            };
        }

        public async Task<ServiceResult<bool>> DeleteEmployeeAsync(int companyId, int employeeId)
        {
            var employee = await _employeeRepo.GetOneAsync(e => e.Id == employeeId && e.CompanyId == companyId);
            if (employee == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "Employee not found."
                };
            }

            var user = await _userManager.FindByIdAsync(employee.UserId);

            try
            {

                if (user != null)
                {
                    var userResult = await _userManager.DeleteAsync(user);
                    if (!userResult.Succeeded)
                    {
                        return new ServiceResult<bool>
                        {
                            IsSuccess = false,
                            Message = string.Join(", ", userResult.Errors.Select(e => e.Description))
                        };
                    }
                }
                

                return new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Message = "Employee deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = $"Delete operation failed: {ex.Message}"
                };
            }
        }

    }
}
