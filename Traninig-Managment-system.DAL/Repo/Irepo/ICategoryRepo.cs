
namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICategoryRepo:IRepo<CourseCategory>
    {
        Task<IEnumerable<CourseCategory>> GetAllCategory(int companyid);

        Task AddCourseCategory(int CompanyId);
    }
}
