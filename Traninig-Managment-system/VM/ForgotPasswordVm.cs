using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.VM
{
    public class ForgotPasswordVm
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
