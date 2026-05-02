namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorWorkspaceService
    {
        Task<InstructorDashboardVm?> GetDashboardAsync(string userId);
        Task<InstructorCourseDetailsVm?> GetCourseDetailsAsync(int courseId, string userId);
        Task<InstructorChapterFormVm?> BuildChapterCreateModelAsync(int courseId, string userId);
        Task<InstructorChapterFormVm?> GetChapterForEditAsync(int chapterId, string userId);
        Task<ServiceResult<int>> CreateChapterAsync(InstructorChapterFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateChapterAsync(InstructorChapterFormVm model, string userId);
        Task<ServiceResult<bool>> DeleteChapterAsync(int chapterId, string userId);
        Task<InstructorLessonFormVm?> BuildLessonCreateModelAsync(int courseId, string userId, int? chapterId = null);
        Task<InstructorLessonFormVm?> GetLessonForEditAsync(int lessonId, string userId);
        Task<ServiceResult<int>> CreateLessonAsync(InstructorLessonFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateLessonAsync(InstructorLessonFormVm model, string userId);
        Task<ServiceResult<string?>> DeleteLessonAsync(int lessonId, string userId);
        Task<IEnumerable<InstructorEmployeeProgressVm>> GetEmployeeProgressAsync(string userId, int? courseId = null);
        Task<InstructorEmployeeDetailsVm?> GetEmployeeDetailsAsync(int employeeId, string userId);
        Task<InstructorExamFormVm?> BuildExamCreateModelAsync(int courseId, string userId, int? chapterId = null);
        Task<InstructorExamFormVm?> GetExamForEditAsync(int examId, string userId);
        Task<ServiceResult<int>> CreateExamAsync(InstructorExamFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateExamAsync(InstructorExamFormVm model, string userId);
        Task<ServiceResult<bool>> DeleteExamAsync(int examId, string userId);
        Task<ServiceResult<bool>> ToggleExamPublishAsync(int examId, string userId);
    }
}
