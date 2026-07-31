using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.View_Model.Company.Company_Category
{
    public class CreateCategoryVm
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 100 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
