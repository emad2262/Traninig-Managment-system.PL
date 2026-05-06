
namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICategoryRepo : IRepo<Category>
    {
        Task<List<Category>> GetCompanyCategoriesWithCoursesAsync(int companyId);
        Task<Category?> GetCompanyCategoryWithCoursesAsync(int companyId, int categoryId);
        Task<bool> ExistsWithNameAsync(int companyId, int excludedCategoryId, string normalizedName);
    }
}
