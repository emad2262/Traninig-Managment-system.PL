namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDisplayVM>> GetCategoriesByCompanyAsync(int companyId);
        Task<CategoryAndCoursesDto?> GetCategoryByIdAsync(int companyId, int categoryId);
        Task<CreateCategoryVM?> GetCategoryForEditAsync(int companyId, int categoryId);
        Task<ServiceResult<int>> CreateCategoryAsync(CreateCategoryVM model, int companyId);
        Task<ServiceResult<bool>> UpdateCategoryAsync(CreateCategoryVM model, int companyId);
        Task<ServiceResult<bool>> DeleteCategoryAsync(int categoryId, int companyId);
    }
}
