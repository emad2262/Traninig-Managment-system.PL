using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CreateCategoryVM
    {
        public int Id { get; set; }

        [Display(Name = "اسم القسم")]
        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم القسم يجب أن يكون بين 2 و 100 حرف")]
        public string Name { get; set; } = string.Empty;

        public int CompanyId { get; set; }
    }
}
