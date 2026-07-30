using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.VM
{
    public class ResetPasswordVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
