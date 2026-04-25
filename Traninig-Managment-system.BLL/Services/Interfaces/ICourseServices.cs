namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICourseServices
    {
        Task<IEnumerable<CourseDto>> GetAllInCategoryAsync(int companyId, int CategoryId);
        Task<CourseDto> GetByIdAsync(int id, int companyId);
        Task<ServiceResult<int>> CreateCourseAsync(CourseDto dto, int companyId);
        Task<ServiceResult<bool>> UpdateAsync(CourseDto dto, int companyId);
        Task<ServiceResult<bool>> DeleteAsync(int id, int companyId);
        Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId);

    }
}
