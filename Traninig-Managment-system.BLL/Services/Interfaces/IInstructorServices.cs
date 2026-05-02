

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorServices
    {
        public Task<IEnumerable<ListInstructorVm>> GetListInstructorAsync(int companyId);
        public Task<InstructorDetails?> GetInstructorDetailsAsync(int companyId, int id);

        public Task<ServiceResult<int>> CreateInstructorAsync(int companyId, CreateInstructorVm model);
        public Task<ServiceResult<bool>> EditInstructorAsync(int companyId, EditInstructorVm model);
        public Task<ServiceResult<bool>> DeleteInstructorAsync(int companyId, int id);


    }
}
