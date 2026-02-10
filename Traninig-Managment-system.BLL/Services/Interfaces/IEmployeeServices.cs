using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.BLL.ModelVm;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeServices
    {
        Task<bool> AddEmployee(CreateEmployeeVm model, int CompanyId);
        Task<IEnumerable<ListEmployeeVm>> GetListEmployeeAsync(int CompanyId);
        Task<EditEmployeeVm> GetEmployeeByIdAsync(int employeeId, int companyId);
        Task<bool> EditEmployeeAsync(EditEmployeeVm model,int CompanyId);
        Task<bool> Delete(int employeeId, int companyId);

        Task<IEnumerable<ListEmployeeVm>> GetEmployeesForInstructorCoursesAsync(int companyId, string instructorUserId);

    }
}
