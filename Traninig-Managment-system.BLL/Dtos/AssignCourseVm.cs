using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class AssignCourseVm
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public IEnumerable<CourseDto> AvailableCourses { get; set; } = new List<CourseDto>();

        public IEnumerable<EmployeeCourseVm> AssignedCourses { get; set; } = new List<EmployeeCourseVm>();

        [Required(ErrorMessage = "Please select at least one course.")]
        [Display(Name = "Selected Courses")]
        public List<int> SelectedCourseIds { get; set; } = new();

        public string Search { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public IEnumerable<CategoryDisplayVM> Categories { get; set; } = new List<CategoryDisplayVM>();
    }
}
