using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorServices
    {
        Task<bool> AddInstructor(CreaeInstructorVm model, int CompanyId);
        Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int CompanyId);
        Task<EditInstructorVm> GetinsturactorByIdAsync(int InstructorId, int companyId);
        Task<bool> EditinsturactorByIdAsync(EditInstructorVm model, int CompanyId);
        Task<bool> Delete(int InstructorVm, int companyId);
    }
}
