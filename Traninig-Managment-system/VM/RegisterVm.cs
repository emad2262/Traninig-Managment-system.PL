using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.VM
{
    public class RegisterVm
    {
        [Required(ErrorMessage = "Name of copany is Required")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emaii Is Required")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب اختيار الباقة")]
        public int PlanId { get; set; } // عشان نعرف هو مختار باقة إيه
    }
}
