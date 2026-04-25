using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CreateInstructorVm
    {
        [Required (ErrorMessage ="Name is required")]
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "password didnt match")]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; }
        

    }
}
