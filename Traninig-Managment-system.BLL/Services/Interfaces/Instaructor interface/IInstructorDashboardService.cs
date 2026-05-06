namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorDashboardService
    {
        Task<InstructorDashboardVm?> GetDashboardAsync(string userId);
        Task<InstructorCourseDetailsVm?> GetCourseDetailsAsync(int courseId, string userId);
    }
}
