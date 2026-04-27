using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeManagementService
    {
        Task<ServiceResult<int>> AddEmployeeAsync(AddEmployeeVm model, int companyId);
        Task<IEnumerable<ListEmployeeVm>> GetEmployeesWithCoursesCountAsync(int companyId);
        Task<EmployeeDetailsVm> GetEmployeeByIdAsync(int CompanyId, int EmployeeId);
        Task<AssignCourseVm?> GetAssignCourseDataAsync(int companyId, int employeeId, string? search = null, int? categoryId = null);
        Task<ServiceResult<int>> AssignCoursesToEmployeeAsync(int companyId, int employeeId, IEnumerable<int> courseIds);

    }
}
