namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeWorkspaceService
    {
        Task<EmployeeDashboardVm?> GetDashboardAsync(string userId);
        Task<EmployeeCourseDetailsVm?> GetCourseDetailsAsync(string userId, int courseId);
        Task<EmployeeLessonWatchVm?> GetLessonAsync(string userId, int lessonId);
        Task<ServiceResult<EmployeeLessonCompletionResultVm>> MarkLessonCompletedAsync(string userId, int lessonId);
        Task<EmployeeExamTakeVm?> GetExamAsync(string userId, int examId);
        Task<ServiceResult<EmployeeExamResultVm>> SubmitExamAsync(string userId, EmployeeExamSubmissionVm model);
        Task<EmployeeCertificateVm?> GetCertificateAsync(string userId, int courseId);
    }
}
