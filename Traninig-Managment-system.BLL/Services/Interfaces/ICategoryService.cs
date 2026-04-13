using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<bool> CreateCategoryAsync(CreateCategoryVM model);
        Task<IEnumerable<CategoryDisplayVM>> GetCategoriesByCompanyAsync(int companyId);
    }
}
