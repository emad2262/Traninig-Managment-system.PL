using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class RegisterVm
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string passwood { get; set; }
        [Required]
        [Compare(nameof(passwood))]
        public string ConfirmePassword { get; set; }

        public bool Check { get; set; }

    }
}
