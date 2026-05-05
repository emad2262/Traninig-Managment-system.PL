namespace Traninig_Managment_system.DAL.Model
{
    public class PlanFeature
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsHighlighted { get; set; }

        public int SortOrder { get; set; }

        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;
    }
}
