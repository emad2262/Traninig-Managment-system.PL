
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Helper;
using Traninig_Managment_system.DAL.Data;


namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeServices : IEmployeeServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeServices(UserManager<ApplicationUser> userManager,ApplicationDbContext context, IEmployeeRepo employeeRepo, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
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

        public async Task<IEnumerable<ListEmployeeVm>> GetEmployeesForInstructorCoursesAsync(int companyId, string instructorUserId)
        {
            var employees = await _employeeRepo.GetEmployeesForInstructorCoursesAsync(companyId, instructorUserId);

            return employees.Select(e => new ListEmployeeVm
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                IsActive = e.IsActive,
                Points = e.Points,
                EnrolledCourses = e.EmployeeCourses
                    .Where(ec => ec.Course.Instructor.UserId == instructorUserId)
                    .Select(ec => new EmployeeCourseInfo
                    {
                        CourseName = ec.Course.Title,
                    }).ToList()
            }).ToList();
        }
        public async Task<AssignEmployeeCoursesVm> GetAssignCoursesForEmployeeAsync(int employeeId, int companyId)
        {
            var employee = await _context.employees
                .FirstOrDefaultAsync(e =>
                    e.Id == employeeId &&
                    e.CompanyId == companyId);

            if (employee == null)
                return null;

            var courses = await _context.courses
                .Where(c => c.Category.CompanyId == companyId)
                .ToListAsync();

            var assignedCourseIds = await _context.EmployeeCourses
                .Where(ec => ec.EmployeeId == employeeId)
                .Select(ec => ec.CourseId)
                .ToListAsync();

            var vm = new AssignEmployeeCoursesVm
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                Courses = courses.Select(c => new CourseAssignItemVm
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    IsAssigned = assignedCourseIds.Contains(c.Id)
                }).ToList()
            };

            return vm;
        }
        public async Task<bool> AssignCourseToEmployeeAsync(int courseId, int employeeId)
        {
            // 1) employee موجود؟
            var employee = await _context.employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return false;

            // 2) course موجود؟ + مربوط بـ Category علشان نجيب CompanyId
            var course = await _context.courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return false;

            // 3) نفس الشركة؟
            if (course.Category.CompanyId != employee.CompanyId)
                return false;

            // 4) منع التكرار
            var alreadyAssigned = await _context.EmployeeCourses
                .AnyAsync(ec => ec.EmployeeId == employeeId && ec.CourseId == courseId);

            if (alreadyAssigned)
                return true;

            // 5) إضافة السجل
            _context.EmployeeCourses.Add(new EmployeeCourse
            {
                EmployeeId = employeeId,
                CourseId = courseId,
                IsCompleted = false,
                AssignedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return true;
        }


    }
}
