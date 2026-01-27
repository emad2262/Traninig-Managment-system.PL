namespace Traninig_Managment_system.VM
{
    public class UserWithRoleVm
    {
        public int TotalUser { get; set; }
        public int totalactive { get; set; }
        public int Totalroles { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new(); // ✅ List مش string
        public bool IsActive { get; set; }
    }
}
