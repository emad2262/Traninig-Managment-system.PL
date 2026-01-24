namespace Traninig_Managment_system.DAL.Model
{
    public class AdminCompany
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string Permissions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; } = null!;
    }
}
