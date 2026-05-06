namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeExamService
    {
        Task<EmployeeExamTakeVm?> GetExamAsync(string userId, int examId);
        Task<ServiceResult<EmployeeExamResultVm>> SubmitExamAsync(string userId, EmployeeExamSubmissionVm model);
    }
}
