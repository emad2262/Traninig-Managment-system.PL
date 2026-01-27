
namespace Traninig_Managment_system.DAL.Model
{
    public class AdminPlatform
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Plan> plans { get; set; }
    }
}
