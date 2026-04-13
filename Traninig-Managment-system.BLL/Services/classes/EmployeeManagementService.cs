using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeRepo _employeeRepo;

        public EmployeeManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            IEmployeeRepo employeeRepo)
        {
            _userManager = userManager;
            _context = context;
            _employeeRepo = employeeRepo;
        }

        public async Task<(bool IsSuccess, string Message)> AddEmployeeAsync(AddEmployeeVm model, int companyId)
        {
            // 1. التأكد إن الإيميل مش متسجل قبل كده
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return (false, "This email is already registered.");


            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                CompanyId = companyId
            };
            var createResult = await _userManager.CreateAsync(newUser, model.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // 3. إعطاء رول الموظف
            await _userManager.AddToRoleAsync(newUser,SD.Employee);

            // 4. إنشاء سجل الموظف في الداتابيز للبيزنس
            var employee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                JobTitle = model.JobTitle ?? string.Empty,
                CompanyId = companyId,
                UserId = newUser.Id,
                IsActive = true,
                Points = 0
            };

            try
            {
                await _context.employees.AddAsync(employee);
                await _context.SaveChangesAsync();

                return (true, "Employee added successfully.");
            }
            catch (Exception ex)
            {
                // لو حصلت مشكلة في جدول الموظفين، امسح اليوزر اللي اتكريت في الـ Identity عشان الداتا متبقاش "يتيمة"
                await _userManager.DeleteAsync(newUser);
                return (false, "Error saving employee to business database: " + ex.Message);
            }
        }


        public async Task<IEnumerable<ListEmployeeVm>> GetListEmployeeAsync(int companyId)
        {
            var employees = await _employeeRepo.GetAllAsync(e => e.CompanyId == companyId);
            return employees.Select(e => new ListEmployeeVm
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                JobTitle = e.JobTitle,
                IsActive = e.IsActive,
                Points = e.Points,
                CoursesCount = _context.EmployeeCourses.Count(ec => ec.EmployeeId == e.Id)
            });
          
        }

        public async Task<EmployeeDetailsVm> GetEmployeeByIdAsync(int CompanyId, int EmployeeId)
        {
            var employee =await _employeeRepo.GetOneAsync(e => e.Id == EmployeeId&&e.CompanyId==CompanyId);


            var employeeVm = new EmployeeDetailsVm
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive,
                Points = employee.Points,
                courses = employee.EmployeeCourses.ToList()
            };
            return employeeVm;
        }
    }
}