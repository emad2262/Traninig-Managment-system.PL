using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Course;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ICourseServices
    {
        Task<IEnumerable<ListCourseDto>> GetAllCoursesInCategoryAsync(int companyId, int categoryId);
        Task<IEnumerable<ListCourseDto>> GetCompanyCoursesAsync(int companyId);
        Task<CourseDetailsDto> GetCourseDetailsAsync(int companyId, int id);
        Task<UpdateCourseDto?> GetCourseForEditAsync(int companyId, int id);
        Task<ServiceResult<int>> CreateCourseAsync(CreateCourseDto dto, int companyId);
        Task<ServiceResult<bool>> EditCourseAsync(UpdateCourseDto model, int companyId);
        Task<ServiceResult<DeletedCourseFilesDto>> DeleteCourseAsync(int id, int companyId);
        Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId);
        Task <int>CourseCount(int companyId);

        Task<int> PublishedCourseCount(int companyId);

    }
}
