namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeLessonService
    {
        Task<EmployeeLessonWatchVm?> GetLessonAsync(string userId, int lessonId);
        Task<ServiceResult<EmployeeLessonCompletionResultVm>> MarkLessonCompletedAsync(string userId, int lessonId);
    }
}
