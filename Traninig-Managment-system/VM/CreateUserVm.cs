namespace Traninig_Managment_system.VM
{
    public class CreateUserVm
    {

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string SelectedRole { get; set; } = null!;
    }
}
