

using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Instructor;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorServices
    {
        Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int companyId);
        Task<InstructorDetailsDto> GetInstructorDetailsAsync(int companyId, int id);

        Task<ServiceResult<int>> CreateInstructorAsync(int companyId, CreateInstructorDto model);
        Task<bool> EditInstructorAsync(int companyId, EditInstructorDto model);
        Task<bool> DeleteInstructorAsync(int companyId, int id);

    }
}
