
using Traninig_Managment_system.BLL.Helper;


namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeServices : IEmployeeServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeServices(UserManager<ApplicationUser> userManager, IEmployeeRepo employeeRepo, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _employeeRepo = employeeRepo;
            _roleManager = roleManager;
        }

        // ==================== index ====================
        public async Task<IEnumerable<ListEmployeeVm>> GetListEmployeeAsync(int CompanyId)
        {
            // جلب كل الموظفين التابعين للشركه
            var employees = await _employeeRepo.GetAllAsync(e => e.CompanyId == CompanyId);

            var result = employees.Select(e => new ListEmployeeVm
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                JobTitle = e.JobTitle,
                IsActive = e.IsActive,
                Points = e.Points
            });

            return result;
        }
        // ==================== create ====================
        public async Task<bool> AddEmployee(CreateEmployeeVm model, int companyId)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return false;

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return false;

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CompanyId = companyId
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
                return false;

            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                JobTitle = model.JobTitle,
                IsActive = model.IsActive,
                CompanyId = companyId,
                UserId = user.Id
            };

            var employeeCreated = await _employeeRepo.CreateAsync(employee);
            if (!employeeCreated)
            {
                await _userManager.DeleteAsync(user);
                return false;
            }

            await _userManager.AddToRoleAsync(user, SD.Employee);
            return true;
        }


        // ==================== Edit (GET) ====================
        public async Task<EditEmployeeVm?> GetEmployeeByIdAsync(int employeeId, int companyId)
        {
            //بجيب الموظف بناء علي الايدي بتاعه والشركه بتاعته عشان ماحدش يقدر يعدل علي موظف مش بتاع شركته
            var employee = await _employeeRepo.GetOneAsync(
                e => e.Id == employeeId && e.CompanyId == companyId);

            if (employee == null)
                return null;

            return new EditEmployeeVm
            {
                Id = employee.Id,
                Name = employee.Name,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive
            };
        }

        // ==================== Edit (POST) ====================
        public async Task<bool> EditEmployeeAsync(EditEmployeeVm model, int companyId)
        {
            var employee = await _employeeRepo.GetOneAsync(
                e => e.Id == model.Id && e.CompanyId == companyId
            );

            if (employee == null)
                return false;

            employee.Id = model.Id;
            employee.Email = model.Email; 
            employee.Name = model.Name;
            employee.JobTitle = model.JobTitle;
            employee.IsActive = model.IsActive;

            return await _employeeRepo.UdateAsync(employee);
        }
        // ====================delete ====================
        public async Task<bool> Delete(int employeeId, int companyId)
        {
            var employee = await _employeeRepo.GetOneAsync(
                e => e.Id == employeeId && e.CompanyId == companyId
            );

            if (employee == null)
                return false;

            //  احذف اليوزر من Identity
            var user = await _userManager.FindByIdAsync(employee.UserId);
            if (user != null)
            {
                var identityResult = await _userManager.DeleteAsync(user);
                if (!identityResult.Succeeded)
                    return false;
            }

            // 2️⃣ احذف الموظف
            await _employeeRepo.Delete(employee);
            return true;
        }
    }
}
