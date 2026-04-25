

namespace Traninig_Managment_system.BLL.Dtos
{
    public class EmployeeDetailsVm
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; }= string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public double Points { get; set; }

        public bool IsActive { get; set; } = true;
        public List<EmployeeCourseVm> courses { get; set; } = new();
    }
}
