

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IEmployeeRepo:IRepo<Employee>
    {
        Task<Employee?> GetWithCoursesAndInstructors(int employeeId);

    }
}
