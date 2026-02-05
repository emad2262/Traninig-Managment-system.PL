using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanyServices
    {
        Task<Company> CreateAsync(Company company);
        Task<Company?> GetByIdAsync(int id);
    }
}
