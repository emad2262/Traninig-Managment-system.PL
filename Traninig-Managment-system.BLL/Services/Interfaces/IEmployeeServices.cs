using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
        Task<AssignEmployeeCoursesVm?> GetAssignCoursesForEmployeeAsync(int employeeId, int companyId);

        Task<bool> AssignCourseToEmployeeAsync(int courseId, int employeeId);

        


    }
}
