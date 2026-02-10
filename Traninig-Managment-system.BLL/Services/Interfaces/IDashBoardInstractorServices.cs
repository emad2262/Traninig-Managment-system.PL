

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IDashBoardInstractorServices
    {
        Task<InstractorDashVm> GetDashboardAsync(int companyId, string instructorUserId);

    }
}
