using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICompanyServices
    {
        Task<CompanyOverviewVm> GetCompanyOverviewAsync(int companyId);
        Task<Company> CreateAsync(Company company);
        Task<Company?> GetByIdAsync(int id);

    }
}
