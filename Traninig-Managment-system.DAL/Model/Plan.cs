namespace Traninig_Managment_system.DAL.Model
{
    public class Plan
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public double Price { get; set; }

        public int DurationInDays { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedByAdminId { get; set; }

        [ForeignKey(nameof(CreatedByAdminId))]
        public virtual AdminPlatform? CreatedByAdmin { get; set; }
        public ICollection<Company> Companys { get; set; }=new List<Company>();
        public ICollection<PlanFeature> PlanFeatures { get; set; }= new List<PlanFeature>();

    }
}
