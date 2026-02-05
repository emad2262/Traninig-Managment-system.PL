
using Microsoft.AspNetCore.Identity;

namespace Traninig_Managment_system.DAL.Model
{
    public class ApplicationUser:IdentityUser
    {
        public int? CompanyId { get; set; }
        public  Company Company { get; set; }
        public  Instructor? Instructor { get; set; }
        public Employee? EmployeeProfile { get; set; }

    }
}
