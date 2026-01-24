

namespace Traninig_Managment_system.DAL.Model
{
    public class Feature
    {

        [Key]
        public int Id { get; set; }


        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }


        [Required]
        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public ICollection<PlanFeature> PlanFeatures { get; set; }= new List<PlanFeature>();

    }
}
