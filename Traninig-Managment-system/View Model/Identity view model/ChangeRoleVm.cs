using Microsoft.AspNetCore.Mvc.Rendering;

namespace Traninig_Managment_system.VM
{
    public class ChangeRoleVm
    {
        public string UserId { get; set; } = null!;

        // للعرض فقط
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? CurrentRole { get; set; }

        // اللي هيتغير
        public string SelectedRole { get; set; } = null!;

        // الدروب داون
        public List<SelectListItem> Roles { get; set; } = new();
    }
}
