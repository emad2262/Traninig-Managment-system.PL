namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorContentService
    {
        Task<InstructorChapterFormVm?> BuildChapterCreateModelAsync(int courseId, string userId);
        Task<InstructorChapterFormVm?> GetChapterForEditAsync(int chapterId, string userId);
        Task<ServiceResult<int>> CreateChapterAsync(InstructorChapterFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateChapterAsync(InstructorChapterFormVm model, string userId);
        Task<ServiceResult<bool>> DeleteChapterAsync(int chapterId, string userId);
        Task<InstructorLessonFormVm?> BuildLessonCreateModelAsync(int courseId, string userId, int? chapterId = null);
        Task<InstructorLessonFormVm?> GetLessonForEditAsync(int lessonId, string userId);
        Task<ServiceResult<int>> CreateLessonAsync(InstructorLessonFormVm model, string userId);
        Task<ServiceResult<bool>> UpdateLessonAsync(InstructorLessonFormVm model, string userId);
        Task<ServiceResult<List<string>>> DeleteLessonAsync(int lessonId, string userId);
    }
}
