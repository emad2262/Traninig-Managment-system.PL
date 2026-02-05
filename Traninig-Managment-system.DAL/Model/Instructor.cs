namespace Traninig_Managment_system.DAL.Model
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Specialization { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 One-to-One with ApplicationUser
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        // 🔗 Company
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public ICollection<Courses> Courses { get; set; } = new List<Courses>();
    }
}
