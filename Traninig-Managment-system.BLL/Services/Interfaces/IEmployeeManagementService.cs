using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeManagementService
    {
        Task<(bool IsSuccess, string Message)> AddEmployeeAsync(AddEmployeeVm model, int companyId);
        Task<IEnumerable<ListEmployeeVm>> GetListEmployeeAsync(int companyId);
        Task<EmployeeDetailsVm> GetEmployeeByIdAsync(int CompanyId, int EmployeeId);

    }
}
