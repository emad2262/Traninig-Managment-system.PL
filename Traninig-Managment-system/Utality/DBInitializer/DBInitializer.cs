namespace Traninig_Managment_system.Utality.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DBInitializer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            try
            {
                // Apply migrations
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                // Roles
                if (!_roleManager.RoleExistsAsync(SD.SuperAdmin).GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new IdentityRole(SD.SuperAdmin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.CompanyAdmin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Instructor)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Employee)).GetAwaiter().GetResult();
               
                // SuperAdmin User
                var adminEmail = "Admin@eraasoft.com";

                var user = _userManager.FindByEmailAsync(adminEmail)
                    .GetAwaiter().GetResult();

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = adminEmail,
                        UserName = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = _userManager.CreateAsync(user, "Admin123$")
                        .GetAwaiter().GetResult();

                    if (result.Succeeded)
                    {
                        _userManager.AddToRoleAsync(user, SD.SuperAdmin)
                            .GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
