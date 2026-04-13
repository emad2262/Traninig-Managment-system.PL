using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CreateCategoryVM
    {
        [Required(ErrorMessage = "Name Is required")]
        [MaxLength(100, ErrorMessage = "الاسم لا يجب أن يتخطى 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }
    }
}
