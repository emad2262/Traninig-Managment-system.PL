

using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class AddEmployeeVm
    {
        [Required(ErrorMessage = "Employee name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        public string? JobTitle { get; set; }

        [Required(ErrorMessage = "Initial password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "password didnt match")]
        [Compare (nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
