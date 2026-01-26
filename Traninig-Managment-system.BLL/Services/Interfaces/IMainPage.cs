using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IMainPage
    {
        Task<IEnumerable<Plan>> GetPlansAsync();
        Task<IEnumerable<Company>> GetCompaniesAsync();
    }
}
