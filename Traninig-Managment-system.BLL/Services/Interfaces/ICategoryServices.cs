
namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICategoryServices
    {
        Task<IEnumerable<CategoryVm>> GetCategoriesInCompany(int CompanyId);
        Task AddCategories(int CompanyId,CreateCategoryVm categoryVm);

        Task DeleteCategories(int CompanyId, int categoryId);
        Task<IEnumerable<CategoryVm>> GetCategoriesForInstructorAsync(int companyId, string instructorUserId);
    }
}
