
namespace Traninig_Managment_system.BLL.Services.classes
{
    public class DashBoardInstractorServices : IDashBoardInstractorServices
    {
        private readonly ICategoryServices _categoryServices;
        private readonly IEmployeeServices _employeeServices;

        public DashBoardInstractorServices(ICategoryServices categoryServices,IEmployeeServices employeeServices)
        {
            _categoryServices = categoryServices;
            _employeeServices = employeeServices;
        }
        public async Task<InstractorDashVm> GetDashboardAsync(int companyId, string instructorUserId)
        {
            var categories = (await _categoryServices.GetCategoriesForInstructorAsync(companyId, instructorUserId)).ToList();

            var employees = (await _employeeServices.GetEmployeesForInstructorCoursesAsync(companyId, instructorUserId)).ToList();

            return new InstractorDashVm
            {
                Categories = categories,
                Employees = employees,
                TotalCategories = categories.Count,
                TotalCourses = categories.Sum(c => c.Courses.Count),
                TotalEmployees = employees.Count
            };
        }
    }
}
