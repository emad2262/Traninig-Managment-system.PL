

using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Employee;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeManagementService
    {
        Task<ServiceResult<int>> CreateEmployeeAsync(CreateEmployeDto model, int companyId);
        Task<IEnumerable<ListEmployeeDto>> GetListEmployees(int companyId);
        Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int employeeId, int companyid);
        Task<int> EmployeeCount(int companyId);

    }
}
