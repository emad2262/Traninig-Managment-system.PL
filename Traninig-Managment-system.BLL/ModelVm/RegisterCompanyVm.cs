using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class RegisterCompanyVm
    {
            // ===== User Data =====
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }
    
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    
        [Required(ErrorMessage = "Confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    
        // ===== Company Data =====
        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
    
        [Phone]
        public string? Phone { get; set; }
    
        public string? Address { get; set; }

    }
}
