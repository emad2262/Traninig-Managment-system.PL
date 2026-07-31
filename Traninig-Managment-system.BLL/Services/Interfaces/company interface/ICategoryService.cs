namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryListDto>> GetAllCategoyr(int companyId);
        Task<CategoryDetailsDto?> GetCategoryByIdAsync(int companyId, int categoryId);
        Task<int> CreateCategoryAsync(CreateCategoryDto model, int companyId);
        Task UpdateCategoryAsync(int companyId, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int categoryId, int companyId);
        Task<int> CategoryCount(int companyId);
    }
}
