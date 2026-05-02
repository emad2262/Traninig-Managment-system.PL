using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICourseRepo : IRepo<Course>
    {
        Task<bool> DeleteCourseWithRelatedDataAsync(int courseId);
    }
}
