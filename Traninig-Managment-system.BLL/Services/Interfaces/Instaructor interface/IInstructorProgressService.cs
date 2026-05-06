namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IInstructorProgressService
    {
        Task<IEnumerable<InstructorEmployeeProgressVm>> GetEmployeeProgressAsync(string userId, int? courseId = null);
        Task<InstructorEmployeeDetailsVm?> GetEmployeeDetailsAsync(int employeeId, string userId);
    }
}
