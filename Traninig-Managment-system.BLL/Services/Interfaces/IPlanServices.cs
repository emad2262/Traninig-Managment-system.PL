
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<Plan>> GetAllAsync();
        Task<Plan?> GetByIdAsync(int id);
        Task<bool> AddAsync(Plan plan);
        Task<bool> UpdateAsync(Plan plan);
        Task<bool> DeleteAsync(int id);
    }
}
