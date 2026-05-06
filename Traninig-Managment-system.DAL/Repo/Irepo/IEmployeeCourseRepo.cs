using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IEmployeeCourseRepo : IRepo<EmployeeCourse>
    {
        Task<List<double>> GetCompanyProgressesAsync(int companyId);
        Task<List<EmployeeCourse>> GetRecentCompanyAssignmentsAsync(int companyId, int take);
        Task<List<EmployeeCourse>> GetRecentCompanyCompletionsAsync(int companyId, int take);
    }
}
