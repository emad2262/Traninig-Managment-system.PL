

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICourseServices
    {
        Task<List<CourseVm>> GetCourse(int categoryId);
        Task AddCourse(int categoryId, CreateCourseVm courseVm);
        Task<CourseDetailsVm?> GetCourseDetails(int courseId);
    }
}
