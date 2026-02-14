

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeLessonServices
    {
        Task<EmployeeDashboardVm> GetEmployeeDashboardAsync(string userId);
        Task<CourseLessons?> GetCourseWithLessonsAsync(string userId, int courseId);

        Task MarkLessonAsCompletedAsync(string userId, int lessonId);
    }
}
