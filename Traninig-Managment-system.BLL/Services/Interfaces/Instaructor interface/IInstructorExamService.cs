namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorExamService
    {
        Task<InstructorExamFormVm?> BuildExamCreateModelAsync(int courseId, string userId, int? chapterId = null);
        Task<InstructorExamFormVm?> GetExamForEditAsync(int examId, string userId);
        Task<ServiceResult<int>> CreateExamAsync(InstructorExamFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateExamAsync(InstructorExamFormVm model, string userId);
        Task<ServiceResult<bool>> DeleteExamAsync(int examId, string userId);
        Task<ServiceResult<bool>> ToggleExamPublishAsync(int examId, string userId);
    }
}
