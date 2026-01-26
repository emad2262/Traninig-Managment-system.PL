using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<IEnumerable<Company>> GetAllCompaniesWithDetailsAsync();
    
    }
}
